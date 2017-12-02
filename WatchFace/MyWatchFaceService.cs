using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Service.Wallpaper;
using Android.Support.Wearable.Watchface;
using Android.Text.Format;
using Android.Util;
using Android.Views;
using Java.Util;
using Java.Util.Concurrent;
using WatchFaceTools;
using Timer = System.Threading.Timer;
using TimeZone = Java.Util.TimeZone;

namespace WatchFace
{
    /// <summary>
    /// MyWatchFaceService implements only one method, OnCreateEngine, 
    /// and it defines a nested class that is derived from CanvasWatchFaceService.Engine.
    /// </summary>
    public class MyWatchFaceService : CanvasWatchFaceService
    {
        // Used for logging:
        private const string Tag = "MyWatchFaceService";

        /// <summary>
        ///   Must be implemented to return a new instance of the wallpaper's engine
        /// </summary>
        /// <returns></returns>
        public override WallpaperService.Engine OnCreateEngine()
        {
            return new MyWatchFaceEngine(this);
        }

        /// <summary>
        ///   Class used for the watch face that draws on the Canvas
        /// </summary>
        private class MyWatchFaceEngine : Engine
        {
            /** Alpha value for drawing time when in mute mode. */
            private const int MUTE_ALPHA = 100;

            /** Alpha value for drawing time when not in mute mode. */
            private const int NORMAL_ALPHA = 255;

            // for blinking colons
            private static readonly long NORMAL_UPDATE_RATE_MS = TimeUnit.Milliseconds.ToMillis(100);
            
            // for when the screen it muted
            private static readonly long MUTE_UPDATE_RATE_MS = TimeUnit.Seconds.ToMillis(60);

            // Reference to the CanvasWatchFaceService that instantiates this engine:
            private readonly CanvasWatchFaceService _owner;

            // The current time:
            private static Calendar _calendar;

            // Bitmaps for drawing the watch face background:
            private Bitmap backgroundBitmap;

            private Bitmap backgroundScaledBitmap;

            private Bitmap aodBackgroundBitmap;
            private Bitmap aodBackgroundScaledBitmap;

            // For painting the hands of the watch:
            private Paint hourPaint;

            // For painting the tick marks around the edge of the clock face:
            private Paint hTickPaint;

            // Bitmaps for drawing on the foreground of the watch face
            private Bitmap hubBitmap;

            private Bitmap hubScaledBitmap;

            // Whether the display supports fewer bits for each color in ambient mode. 
            // When true, we disable anti-aliasing in ambient mode:
            private bool lowBitAmbient;

            private Paint minutePaint;
            private Paint mTickPaint;

            private bool registeredTimezoneReceiver;

            private WatchHand secHand, minHand, hrHand, milHand;
            private Paint secondPaint;

            private Timer timerSeconds;

            // Broadcast receiver for handling time zone changes:
            private TimeZoneReceiver timeZoneReceiver;

            //for inspecting current update rate on timer
            private long _updateRate;

            // Saves a reference to the outer CanvasWatchFaceService
            public MyWatchFaceEngine(CanvasWatchFaceService owner) : base(owner)
            {
                _owner = owner;
            }

            // Called when the engine is created for the first time: 
            public override void OnCreate(ISurfaceHolder holder)
            {
                base.OnCreate(holder);

                // Configure the system UI. Instantiates a WatchFaceStyle object that causes 
                // notifications to appear as small peek cards that are shown only briefly 
                // when interruptive. Also disables the system-style UI time from being drawn:
                SetWatchFaceStyle(new WatchFaceStyle.Builder(_owner)
                    .SetCardPeekMode(WatchFaceStyle
                        .PeekModeShort) // This method is deprecated. Wear 2.0 doesn't have peeking cards
                    .SetBackgroundVisibility(WatchFaceStyle
                        .BackgroundVisibilityInterruptive) // This method is deprecated. Wear 2.0 doesn't have peeking cards
                    .SetShowSystemUiTime(false) //  this will be removed in a future version of the Wear platform
                    .SetStatusBarGravity(Resource.Id.bottom | Resource.Id.right)
                    .Build());


                // Configure the background image:
                var backgroundDrawable = Application.Context.Resources.GetDrawable(
                    Resource.Drawable.mind_the_gap);
                backgroundBitmap = (backgroundDrawable as BitmapDrawable)?.Bitmap;

                var AOD_backgroundDrawable = Application.Context.Resources.GetDrawable(
                    Resource.Drawable.Underground);
                aodBackgroundBitmap = (AOD_backgroundDrawable as BitmapDrawable)?.Bitmap;

                // configure a foreground image for use later (bullet hole)
                var foregroundDrawable =
                    Application.Context.Resources.GetDrawable(
                        Resource.Drawable.BSN_Chevron_Red);
                hubBitmap = (foregroundDrawable as BitmapDrawable)?.Bitmap;

                // Initialize paint objects for drawing the clock hands and tick marks:

                // Hand paints:   
                hourPaint = WatchFaceFactory.GetHourHand(Color.Blue,false);
                minutePaint = WatchFaceFactory.GetMinuteHand(Color.White,false);
                secondPaint = WatchFaceFactory.GetSecondHand(Color.White,false);

                // Ticks:
                hTickPaint = new Paint { AntiAlias = true, StrokeWidth = 3.0f };
                hTickPaint.SetARGB(NORMAL_ALPHA, 255, 0, 0);
                //hTickPaint.SetShadowLayer(1.1f, .5f, .5f, Color.Argb(MUTE_ALPHA, 50, 50, 50));

                mTickPaint = new Paint { AntiAlias = true, StrokeWidth = 1.5f };
                mTickPaint.SetARGB(NORMAL_ALPHA, 255, 0, 0);
                // mTickPaint.SetShadowLayer(1.1f, .5f, .5f, Color.Argb(MUTE_ALPHA, 50, 50, 50));

                // Instantiate the time object:
                _calendar = Calendar.GetInstance(Locale.Default);

                // Start a timer for redrawing the click face (second hand) every second.
                // How to stop the timer? It shouldn't run in ambient mode...
                _updateRate = NORMAL_UPDATE_RATE_MS;
                timerSeconds = new Timer(state => { Invalidate(); },
                    null, TimeSpan.FromMilliseconds(_updateRate),
                    TimeSpan.FromMilliseconds(_updateRate));
            }
            
            /// <summary>
            /// Called to draw the watch face
            /// </summary>
            /// <param name="canvas"></param>
            /// <param name="bounds"></param>
            public override void OnDraw(Canvas canvas, Rect bounds)
            {
                _calendar = Calendar.GetInstance(Locale.Default);
                // Determine the bounds of the drawing surface:
                var width = bounds.Width();
                var height = bounds.Height();

                // Determine the center of the drawing surface:
                var centerX = width / 2.0f;
                var centerY = height / 2.0f;

                // set a default background color
                canvas.DrawColor(Color.Black);

                // draw the face ticks (if not in ambient mode)
                if (!IsInAmbientMode)
                {
                    // Draw the hour ticks:
                    var hticks = new WatchTicks(centerX, centerY, 25, 5, -5) { TickPaint = hTickPaint };
                    hticks.DrawTicks(canvas, 12); // draw 12 of them.

                    // Draw the minute ticks:
                    var mticks = new WatchTicks(centerX, centerY, 10, 3, -10) { TickPaint = mTickPaint };
                    mticks.DrawTicks(canvas, 60, 5); // draw 60 of them, but skip every 5th one [60/12 = 5]
                }


                var inspectTimerTickRate = _updateRate;
                // draw the background (scaled to fit) along with Ticks (if not in ambient mode)
                if (!IsInAmbientMode)
                {
                    if (backgroundScaledBitmap == null ||
                        backgroundScaledBitmap.Width != width || backgroundScaledBitmap.Height != height)
                        backgroundScaledBitmap =
                            Bitmap.CreateScaledBitmap(backgroundBitmap, width, height, true /* filter */);
                    canvas.DrawBitmap(backgroundScaledBitmap, 0, 0, null);
                }
                else
                {
                    // AOD
                    if (aodBackgroundScaledBitmap == null ||
                        aodBackgroundScaledBitmap.Width != width || aodBackgroundScaledBitmap.Height != height)
                        aodBackgroundScaledBitmap =
                            Bitmap.CreateScaledBitmap(aodBackgroundBitmap, width, height, true /* filter */);

                    // half-alpha
                    canvas.DrawBitmap(aodBackgroundScaledBitmap, 0, 0, new Paint() { Alpha = MUTE_ALPHA });
                }


                

                // draw something with the date (change the color based on AOD)
                var str = DateTime.Now.ToString("ddd, dd MMM");
                var textPaint = new Paint
                {
                    Alpha = NORMAL_ALPHA,
                    AntiAlias = true,
                    Color = ShouldTimerBeRunning() ? Color.White : Color.Silver, // (change the color based on AOD)
                    TextSize = centerY / 10.0f
                };
                var tf = Typeface.Create("Arial", TypefaceStyle.Bold);
                textPaint.SetTypeface(tf);
                // textPaint.SetShadowLayer(1.5f, -1f, -1f, Color.Argb(MUTE_ALPHA, 50, 50, 50));
                var dl = new Coords(centerX - 40, centerY+40);
                canvas.DrawText(str, dl.X, dl.Y, textPaint);
                


                // // draw a central hub
                //var bhW = (int)(width / 4.0f);
                //var bhH = (int)(height / 4.0f);
                //var bhX = centerX - bhW / 2.0f;
                //var bhY = centerY - bhH / 2.0f;
                //if (hubScaledBitmap == null)
                //    hubScaledBitmap = Bitmap.CreateScaledBitmap(hubBitmap, bhW, bhH, true /* filter */);
                //canvas.DrawBitmap(hubScaledBitmap, bhX, bhY, null);

                var secLength = 40;
                var minLength = 30;
                var hrLength = 20;

                if (!IsInAmbientMode)
                {    //// draw the millisecond tick hand - have to include padding (from outside edge, as neg. in pixels)
                    //var milLength = 10;
                    //var milPad = -10;
                    //milHand = new WatchHand(HandType.MILLISECONDS, HandStyle.OUTSIDE, centerX, centerY, milLength,
                    //        milPad)
                    //{ paint = secondPaint };
                    //milHand.DrawHand(canvas, _calendar);


                    // Draw the second hand only in interactive mode:
                    secHand = new WatchHand(HandType.SECONDS, HandStyle.OUTSIDE, centerX, centerY, (int)secLength, -30)
                    {
                        Paint = secondPaint
                    };
                    secHand.DrawHand(canvas, _calendar);

                
                    // Draw the minute hand:
                    minHand = new WatchHand(HandType.MINUTES, HandStyle.OUTSIDE, centerX, centerY, (int)minLength, -30)
                    {
                        Paint = minutePaint
                    };
                    minHand.DrawHand(canvas, _calendar);

                    // Draw the hour hand:
                    hrHand = new WatchHand(HandType.HOURS, HandStyle.OUTSIDE, centerX, centerY, (int)hrLength, -30)
                    {
                        Paint = hourPaint
                    };
                    hrHand.DrawHand(canvas, _calendar);
                }
                else
                {
                    // Draw the outline minute hand:
                    var outerMinP = WatchFaceFactory.GetMinuteHand(Color.White, false);
                    outerMinP.StrokeWidth = minutePaint.StrokeWidth + 4;
                    minHand = new WatchHand(HandType.MINUTES, HandStyle.CENTRIC, centerX, centerY, (int)minLength)
                    {
                       Paint = outerMinP
                    };
                    minHand.DrawHand(canvas, _calendar);

                    // Draw the outline hour hand:
                    var outerHourP = WatchFaceFactory.GetMinuteHand(Color.White, false);
                    outerHourP.StrokeWidth = hourPaint.StrokeWidth + 4;
                    hrHand = new WatchHand(HandType.HOURS, HandStyle.CENTRIC, centerX, centerY, (int)hrLength)
                    {
                        Paint = outerHourP
                    };
                    hrHand.DrawHand(canvas, _calendar);

                    // Draw the minute hand:
                    var innerMinPaint = WatchFaceFactory.GetMinuteHand(Color.Black, false);
                    minHand = new WatchHand(HandType.MINUTES, HandStyle.CENTRIC, centerX, centerY, (int)minLength)
                    {
                        Paint = innerMinPaint
                    };
                    minHand.DrawHand(canvas, _calendar);

                    // Draw the hour hand:
                    var innerHourP = WatchFaceFactory.GetHourHand(Color.Black, false);
                    hrHand = new WatchHand(HandType.HOURS, HandStyle.CENTRIC, centerX, centerY, (int)hrLength)
                    {
                        Paint = innerHourP
                    };
                    hrHand.DrawHand(canvas, _calendar);
                }
            }


            // Called when the properties of the Wear device are determined, specifically 
            // low bit ambient mode (the screen supports fewer bits for each color in
            // ambient mode):
            public override void OnPropertiesChanged(Bundle properties)
            {
                base.OnPropertiesChanged(properties);

                lowBitAmbient = properties.GetBoolean(PropertyLowBitAmbient);

                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "OnPropertiesChanged: low-bit ambient = " + lowBitAmbient);
            }


            /// <summary>
            ///   Called periodically to update the time shown by the watch face:
            ///   at least once per minute in ambient and interactive modes, and whenever the date, time, or timezone has changed:
            /// </summary>
            public override void OnTimeTick()
            {
                base.OnTimeTick();

                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "onTimeTick: ambient = " + IsInAmbientMode);

                Invalidate();
            }

            /// <summary>
            ///   Called when the device enters or exits ambient mode.
            ///   In ambient mode, the watch face disables anti-aliasing while drawing.
            /// </summary>
            /// <param name="inAmbientMode"></param>
            public override void OnAmbientModeChanged(bool inAmbientMode)
            {
                base.OnAmbientModeChanged(inAmbientMode);

                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "OnAmbientMode");

                if (lowBitAmbient)
                {
                    var antiAlias = !inAmbientMode;
                    hourPaint.AntiAlias = antiAlias;
                    minutePaint.AntiAlias = antiAlias;
                    secondPaint.AntiAlias = antiAlias;
                    hTickPaint.AntiAlias = antiAlias;
                    mTickPaint.AntiAlias = antiAlias;
                }
                Invalidate();
            }



            // Called whenever the watch face is becoming visible or hidden. 
            // Note that you must call base.OnVisibilityChanged first:

            public override void OnVisibilityChanged(bool visible)
            {
                base.OnVisibilityChanged(visible);

                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "OnVisibilityChanged: " + visible);

                // If the watch face became visible, register the timezone receiver and get the current time. 

                // Else, unregister the timezone receiver:

                if (visible)
                {
                    RegisterTimezoneReceiver();
                    _calendar.Clear(CalendarField.ZoneOffset);
                    _calendar.TimeZone = TimeZone.GetTimeZone(TimeZone.Default.ID);
                }
                else
                {
                    UnregisterTimezoneReceiver();
                }
            }

            // Run the timer only when visible and in interactive mode:
            private bool ShouldTimerBeRunning()
            {
                return IsVisible && !IsInAmbientMode;
            }

            // Registers the time zone broadcast receiver (defined at the end of 
            // this file) to handle time zone change events:

            private void RegisterTimezoneReceiver()
            {
                if (registeredTimezoneReceiver)
                {
                }
                else
                {
                    if (timeZoneReceiver == null)
                        timeZoneReceiver = new TimeZoneReceiver
                        {
                            Receive = intent =>
                            {
                                _calendar.Clear(CalendarField.ZoneOffset);
                                _calendar.TimeZone = TimeZone.Default;
                                _calendar = Calendar.GetInstance(Locale.Default);
                            }
                        };
                    registeredTimezoneReceiver = true;
                    var filter = new IntentFilter(Intent.ActionTimezoneChanged);
                    Application.Context.RegisterReceiver(timeZoneReceiver, filter);
                }
            }

            // Unregisters the timezone Broadcast receiver:
            private void UnregisterTimezoneReceiver()
            {
                if (!registeredTimezoneReceiver)
                    return;
                registeredTimezoneReceiver = false;
                Application.Context.UnregisterReceiver(timeZoneReceiver);
            }
        }
    }
}