using System;
using System.Collections.Generic;
using System.Threading;
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
using Java.Util.Concurrent;
using TimeZone = Java.Util.TimeZone;

namespace WatchFace
{
    // MyWatchFaceService implements only one method, OnCreateEngine, 
    // and it defines a nested class that is derived from
    // CanvasWatchFaceService.Engine.

    public class MyWatchFaceService : CanvasWatchFaceService
    {
        // Used for logging:
        private const string Tag = "MyWatchFaceService";


        /// <summary>
        /// Must be implemented to return a new instance of the wallpaper's engine
        /// </summary>
        /// <returns></returns>
        public override WallpaperService.Engine OnCreateEngine()
        {
            return new MyWatchFaceEngine(this);
        }


        /// <summary>
        /// Class used for the watch face that draws on the Canvas
        /// </summary>
        private class MyWatchFaceEngine : Engine
        {
            // Update every second:
            private static readonly long InterActiveUpdateRateMs = TimeUnit.Seconds.ToMillis(1);

            // Bitmaps for drawing the watch face background:
            private Bitmap backgroundBitmap;
            private Bitmap backgroundScaledBitmap;

            // Bitmaps for drawing on the foreground of the watch face
            private Bitmap hubBitmap;
            private Bitmap hubScaledBitmap;

            // For painting the hands of the watch:
            private Paint hourPaint;

            // Whether the display supports fewer bits for each color in ambient mode. 
            // When true, we disable anti-aliasing in ambient mode:
            private bool lowBitAmbient;

            private Paint minutePaint;

            // Reference to the CanvasWatchFaceService that instantiates this engine:
            private readonly CanvasWatchFaceService _owner;

            private bool registeredTimezoneReceiver;
            private Paint secondPaint;

            // For painting the tick marks around the edge of the clock face:
            private Paint hTickPaint;
            private Paint mTickPaint;

            // The current time:
            private Time _time;

            private Timer timerSeconds;

            // Broadcast receiver for handling time zone changes:
            private TimeZoneReceiver timeZoneReceiver;

            // Saves a reference to the outer CanvasWatchFaceService
            public MyWatchFaceEngine(CanvasWatchFaceService owner) : base(owner)
            {
                this._owner = owner;
            }

            // Called when the engine is created for the first time: 
            public override void OnCreate(ISurfaceHolder holder)
            {
                base.OnCreate(holder);

                // Configure the system UI. Instantiates a WatchFaceStyle object that causes 
                // notifications to appear as small peek cards that are shown only briefly 
                // when interruptive. Also disables the system-style UI time from being drawn:
                SetWatchFaceStyle(new WatchFaceStyle.Builder(_owner)
                    .SetCardPeekMode(WatchFaceStyle.PeekModeShort)
                    .SetBackgroundVisibility(WatchFaceStyle.BackgroundVisibilityInterruptive)
                    .SetShowSystemUiTime(false)
                    .Build());

                // Configure the background image:
                var backgroundDrawable = Application.Context.Resources.GetDrawable(Resource.Drawable.gwg_background);

                backgroundBitmap = (backgroundDrawable as BitmapDrawable).Bitmap;

                // configure a foreground image for use later (bullet hole)
                var foregroundDrawable =
                    Application.Context.Resources.GetDrawable(Resource.Drawable.bullet_hole);
                hubBitmap = (foregroundDrawable as BitmapDrawable).Bitmap;

                // Initialize paint objects for drawing the clock hands and tick marks:

                // Hour hand:   
                hourPaint = WatchFaceFactory.GetHourHand(Color.White);
                //hourPaint = new Paint();
                //hourPaint.SetARGB(255, 255, 255, 255);
                //hourPaint.StrokeWidth = 5.0f;
                //hourPaint.AntiAlias = true;
                //hourPaint.StrokeCap = Paint.Cap.Round;

                // Minute hand:
                minutePaint = WatchFaceFactory.GetMinuteHand(Color.White);
                //minutePaint = new Paint();
                //minutePaint.SetARGB(255, 255, 255, 255);
                //minutePaint.StrokeWidth = 3.0f;
                //minutePaint.AntiAlias = true;
                //minutePaint.StrokeCap = Paint.Cap.Round;

                // Seconds hand:
                secondPaint = WatchFaceFactory.GetSecondHand(Color.Red);
                //secondPaint = new Paint();
                //secondPaint.SetARGB(255, 255, 0, 0);
                //secondPaint.StrokeWidth = 2.0f;
                //secondPaint.AntiAlias = true;
                //secondPaint.StrokeCap = Paint.Cap.Round;

                // Tick marks around the edge of the face:
                //tickPaint = new Paint();
                //tickPaint.SetARGB(100, 255, 255, 255);
                //tickPaint.StrokeWidth = 2.0f;
                //tickPaint.AntiAlias = true;

                // Instantiate the time object:
                _time = new Time();

                // Start a timer for redrawing the click face (second hand) every second.
                // How to stop the timer? It shouldn't run in ambient mode...
                timerSeconds = new Timer(state =>
                    {
                        Invalidate();
                    }, null,
                    TimeSpan.FromMilliseconds(InterActiveUpdateRateMs),
                    TimeSpan.FromMilliseconds(InterActiveUpdateRateMs));
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
            /// Called periodically to update the time shown by the watch face: 
            /// at least once per minute in ambient and interactive modes, 
            /// and whenever the date, time, or timezone has changed:
            /// </summary>
            public override void OnTimeTick()
            {
                base.OnTimeTick();

                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "onTimeTick: ambient = " + IsInAmbientMode);

                Invalidate();
            }

            /// <summary>
            ///  Called when the device enters or exits ambient mode. 
            /// In ambient mode, the watch face disables anti-aliasing while drawing.
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

            // Called to draw the watch face:

            public override void OnDraw(Canvas canvas, Rect bounds)
            {
                // Get the current time:
                _time.SetToNow();

                // Determine the bounds of the drawing surface:
                var width = bounds.Width();
                var height = bounds.Height();

                // Draw the background, scaled to fit:
                if (backgroundScaledBitmap == null ||
                    backgroundScaledBitmap.Width != width || backgroundScaledBitmap.Height != height)
                    backgroundScaledBitmap = Bitmap.CreateScaledBitmap(backgroundBitmap, width, height, true /* filter */);
                canvas.DrawColor(Color.Black);
                canvas.DrawBitmap(backgroundScaledBitmap, 0, 0, null);

                // Determine the center of the drawing surface:
                var centerX = width / 2.0f;
                var centerY = height / 2.0f;


                // draw a central hub (bullet hole?)
                // Draw the background, scaled to fit:
                int bhW = (int)(width / 4.0f);
                int bhH = (int)(height / 4.0f);
                var bhX = centerX - bhW / 2;
                var bhY = centerY - bhH / 2;
                if (hubScaledBitmap == null)
                    hubScaledBitmap = Bitmap.CreateScaledBitmap(hubBitmap, bhW, bhH, true /* filter */);
                canvas.DrawBitmap(hubScaledBitmap, bhX, bhY, null);


                // Draw the hour ticks:

                var hticks = new WatchTicks(centerX, centerY, 20, 3, 0);
                hticks.TickPaint.SetARGB(255, 210, 0, 0);
                hticks.TickPaint.SetShadowLayer(1.1f, 2f, 2f, Color.Argb(178, 50, 50, 50));
                hticks.DrawTicks(canvas, 12);


                // Draw the minute ticks:
                var mt = new WatchTicks(centerX, centerY, 10, 2, -10);
                mt.TickPaint.SetARGB(255, 159, 191, 255);
                mt.TickPaint.SetShadowLayer(1.1f, 2f, 2f, Color.Argb(178, 50, 50, 50));
                mt.DrawTicks(canvas, 60, 5);


                // Calculate the angle of rotation and length for each hand:
              //  var secRot = WatchFaceFactory.GetSecondHandRotation(_time); // _time.Second / 30f * (float)Math.PI;
                var minutes = _time.Minute;
                var minRot = WatchFaceFactory.GetMinuteHandRotation(_time); // minutes / 30f * (float)Math.PI;
                var hrRot = WatchFaceFactory.GetHourHandRotation(_time); //(_time.Hour + minutes / 60f) / 6f * (float)Math.PI;

                // the lengths of each hand (half-of-face minus some value)
                var secLength = centerX - 20;
                var minLength = centerX - 40;
                var hrLength = centerX - 80;

                // Draw the second hand only in interactive mode:
                if (!IsInAmbientMode)
                {

                    // blur, position x offset, position y offset, color 
                    secondPaint.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50)); // 178 alpha = 70% opacity , (50,50,50) = #323232
                 //   var c = WatchFaceFactory.GetDrawLineStartAndStops(centerX, centerY, secRot, secLength);
                    var secHand = new WatchHand(HandType.SECONDS,centerX, centerY,secLength)
                    {
                        paint = secondPaint,
                        startCoords = new Coords(){X = centerX ,Y=centerY},
                    };
                    secHand.DrawHand(canvas, _time);

                    //canvas.DrawLine(centerX, centerY, c.ePos.X, c.ePos.Y, secondPaint);


                }



                // Draw the minute hand:
                var m = WatchFaceFactory.GetDrawLineStartAndStops(centerX, centerY, minRot, minLength);
                // blur, position x offset, position y offset, color 
                minutePaint.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50)); // 178 alpha = 70% opacity , (50,50,50) = #323232

                canvas.DrawLine(centerX, centerY, m.ePos.X, m.ePos.Y, minutePaint);


                // Draw the hour hand:
                var h = WatchFaceFactory.GetDrawLineStartAndStops(centerX, centerY, hrRot, hrLength);
                // add a dropshadow to the hour hand
                // blur, position x offset, position y offset, color 
                hourPaint.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50)); // 178 alpha = 70% opacity , (50,50,50) = #323232
                canvas.DrawLine(centerX, centerY, h.ePos.X, h.ePos.Y, hourPaint);


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
                    _time.Clear(TimeZone.Default.ID);
                    _time.SetToNow();
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
                    {
                        timeZoneReceiver = new TimeZoneReceiver();
                        timeZoneReceiver.Receive = intent =>
                        {
                            _time.Clear(intent.GetStringExtra("time-zone"));
                            _time.SetToNow();
                        };
                    }
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