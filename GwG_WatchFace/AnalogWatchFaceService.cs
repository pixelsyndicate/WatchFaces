using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Service.Wallpaper;
using Android.Support.Wearable.Watchface;
using Android.Util;
using Android.Views;
using Java.Util;
using WatchFaceTools;

namespace WatchFace
{
    /// <summary>
    ///     AnalogWatchFaceService implements only one method, OnCreateEngine,  
    /// and it defines a nested class that is derived from CanvasWatchFaceService.Engine.
    /// </summary>
    public class AnalogWatchFaceService : CanvasWatchFaceService
    {
        // Used for logging:
        private const string Tag = "AnalogWatchFaceService";

        /// <summary>
        ///   Must be implemented to return a new instance of the wallpaper's engine
        /// </summary>
        /// <returns></returns>
        public override WallpaperService.Engine OnCreateEngine()
        {
            return new AnalogEngine(this);
        }


        /// <summary>
        ///   Class used for the watch face that draws on the Canvas
        /// </summary>
        private class AnalogEngine : Engine
        {
            private static IDateTime _dateTimeAdapter;
            private const int MSG_UPDATE_TIME = 0;

            //  30 frames per second 1000/30 = 33
            //  20 frames per second 1000/20 = 50
            //  15 frames per second 1000/15 = 66
            //  10 frames per second 1000/10 = 100
            private const int INTERACTIVE_UPDATE_RATE_MS = 100;

            // this gets refreshed OnCreate,
            private Java.Util.Calendar _calendar;

            // this will recieve messages from UpdateTimer() or itself
            private static Handler _mUpdateTimeHandler;

            // Broadcast receiver for handling time zone changes:
            private static TimeZoneReceiver _timeZoneReceiver;

            private static bool _registeredTimezoneReceiver;

            // device features
            private bool _hasLowBitAmbient;

            private bool _hasBurnInProtection;

            // graphic objects
            private Bitmap _backgroundBitmap;
            private Bitmap _backgroundScaledBitmap;
            private Bitmap _aodBackgroundBitmap;
            private Bitmap _aodBackgroundScaledBitmap;
            private Bitmap _hubBitmap;
            private Bitmap _hubScaledBitmap;

            private Paint _facePaint;
            private Paint _hourPaint;
            private Paint _minutePaint;
            private Paint _secondPaint;
            private Paint _datePaint;
            private Paint _tickPaint;
            private Paint _minuteTickPaint;

            // hand paint elements
            private const float HAND_END_CAP_RADIUS = 4f;
            private const float SHADOW_RADIUS = 6f;

            private WatchHand _secHand, _minHand, _hrHand, _milHand;
            private float _minLength, _hrLength, _secLength;

            /// <summary>
            /// Use this to instantiate the Handler for _mUpdateTimeHandler and
            /// use this to instantiate the _timeZoneReciever
            /// </summary>
            /// <param name="self"></param>
            private static void Init(AnalogEngine self)
            {
                _dateTimeAdapter = new DateTimeAdapter();
                _mUpdateTimeHandler = new Handler(message =>
                {
                    switch (message.What)
                    {
                        case MSG_UPDATE_TIME:
                            self.Invalidate();
                            if (self.ShouldTimerBeRunning())
                            {
                                long timeMs = _dateTimeAdapter.UnixNow;
                                long delayMs = INTERACTIVE_UPDATE_RATE_MS - (timeMs % INTERACTIVE_UPDATE_RATE_MS);
                                _mUpdateTimeHandler.SendEmptyMessageDelayed(MSG_UPDATE_TIME, delayMs);
                            }
                            break;
                    }
                });

                if (_registeredTimezoneReceiver)
                {

                }
                else
                {
                    if (_timeZoneReceiver == null)
                        _timeZoneReceiver = new TimeZoneReceiver
                        {
                            Receive = intent =>
                            {
                                self._calendar.TimeZone = Java.Util.TimeZone.Default;
                                self.Invalidate();
                            }
                        };
                    _registeredTimezoneReceiver = true;
                    var filter = new IntentFilter(Intent.ActionTimezoneChanged);
                    Application.Context.RegisterReceiver(_timeZoneReceiver, filter);
                }
            }

            public AnalogEngine(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
                Init(this);
            }

            public AnalogEngine(CanvasWatchFaceService self) : base(self)
            {
                _owner = self;
                Init(this);
            }

            // Reference to the CanvasWatchFaceService that instantiates this engine:
            private readonly CanvasWatchFaceService _owner;

            private float _centerX, _centerY;


            /// <summary>
            /// In ambient mode, the system calls the Engine.onTimeTick() method every minute. 
            /// note: this is not the active face timer. that is your custom one.
            /// </summary>
            public override void OnTimeTick()
            {
                base.OnTimeTick();

                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "onTimeTick: ambient = " + IsInAmbientMode);

                // redraw
                Invalidate();
            }

            // Called when the engine is created for the first time: 
            public override void OnCreate(ISurfaceHolder holder)
            {
                base.OnCreate(holder);
                var Res = Application.Context.Resources;
                // CONFIG THE UI
                // configure the system UI
                SetWatchFaceStyle(new WatchFaceStyle.Builder(_owner)
                    .SetBackgroundVisibility(WatchFaceStyle.BackgroundVisibilityInterruptive)
                    .SetShowSystemUiTime(false)
                    .Build());

                // load the background image(s)
                _backgroundBitmap = BitmapFactory.DecodeResource(Res, Resource.Drawable.gwg_background);
                _aodBackgroundBitmap = BitmapFactory.DecodeResource(Res, Resource.Drawable.gwg_aod);

                // configure a foreground image for use later (bullet hole)
                _hubBitmap = BitmapFactory.DecodeResource(Res, Resource.Drawable.bullet_hole);


                // create graphic styles


                // Initialize paint objects for drawing the clock hands and tick marks:
                _facePaint = new Paint { AntiAlias = false, Alpha = 255 };

                // Hand paints:   
                _hourPaint = WatchFaceFactory.GetHourHand(Color.White);

                // change the style on the minute hand
                _minutePaint = new Paint
                {
                    AntiAlias = true,
                    Color = Color.White,
                    StrokeWidth = 4f
                };
                _minutePaint.SetShadowLayer(SHADOW_RADIUS, 0, 0, Color.Black);
                _minutePaint.SetStyle(Paint.Style.Stroke);

                _secondPaint = WatchFaceFactory.GetSecondHand(Color.Red);

                // Ticks:
                _tickPaint = new Paint { AntiAlias = true, StrokeWidth = 3.0f };
                _tickPaint.SetARGB(255, 210, 0, 0);
                _tickPaint.SetShadowLayer(1.1f, .5f, .5f, Color.Argb(120, 50, 50, 50));

                _minuteTickPaint = new Paint { AntiAlias = true, StrokeWidth = 1.5f };
                _minuteTickPaint.SetARGB(255, 159, 191, 255);
                _minuteTickPaint.SetShadowLayer(1.1f, .5f, .5f, Color.Argb(120, 50, 50, 50));

                _datePaint = new Paint { Alpha = 255, AntiAlias = true, };

                // allocate a Calendar to calculate local time using the UTC time and time zone
                _calendar = Java.Util.Calendar.GetInstance(Java.Util.Locale.Default);
            }

            /// <summary>
            /// Use this to dig out some properties about the device screen
            /// You should take these device properties into account when drawing your watch face:
            ///  For devices that use low-bit ambient mode, the screen supports fewer bits for each color in ambient mode, so you should disable anti-aliasing and bitmap filtering when the device switches to ambient mode.
            /// For devices that require burn-in protection, avoid using large blocks of white pixels in ambient mode and do not place content within 10 pixels of the edge of the screen, since the system shifts the content periodically to avoid pixel burn-in.
            /// </summary>
            /// <param name="properties"></param>
            public override void OnPropertiesChanged(Bundle properties)
            {
                base.OnPropertiesChanged(properties);
                _hasLowBitAmbient = properties.GetBoolean(PropertyLowBitAmbient, false);
                _hasBurnInProtection = properties.GetBoolean(PropertyBurnInProtection, false);
                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "OnPropertiesChanged: low-bit ambient = " + _hasLowBitAmbient);
            }

            /// <summary>
            ///   Called when the device enters or exits ambient mode.
            ///   In ambient mode, the watch face disables anti-aliasing while drawing.
            ///  Good place to start any timers
            /// </summary>
            /// <param name="inAmbientMode"></param>
            public override void OnAmbientModeChanged(bool inAmbientMode)
            {
                base.OnAmbientModeChanged(inAmbientMode);

                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "OnAmbientMode");

                if (_hasLowBitAmbient)
                {
                    bool antiAlias = !inAmbientMode;

                    _hourPaint.AntiAlias = (antiAlias);
                    _minutePaint.AntiAlias = (antiAlias);
                    _secondPaint.AntiAlias = (antiAlias);
                    _tickPaint.AntiAlias = (antiAlias);
                }
                Invalidate();
                UpdateTimer(); // kick of the ambient timer
            }

            private void DrawHandAsRoundedRect(Canvas canvas, float handLength, Paint mHandPaint)
            {
                canvas.DrawRoundRect(_centerX - HAND_END_CAP_RADIUS, _centerY - handLength,
                    _centerX + HAND_END_CAP_RADIUS, _centerY + HAND_END_CAP_RADIUS,
                    HAND_END_CAP_RADIUS, HAND_END_CAP_RADIUS, mHandPaint);
            }

            /// <summary>
            /// Called to draw the watch face
            /// To achieve a high frame rate, which results in smooth animations, no intense computations should happen in onDraw
            /// </summary>
            /// <param name="canvas"></param>
            /// <param name="bounds"></param>
            public override void OnDraw(Canvas canvas, Rect bounds)
            {
                // refresh calendar 
                _calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);

                // check the time to see how long this drawing is going to take
                long frameStartTimeMs = SystemClock.ElapsedRealtime();

                // Determine the bounds of the drawing surface:
                var width = bounds.Width();
                var height = bounds.Height();

                // Determine the center of the drawing surface:
                _centerX = width / 2.0f;
                _centerY = height / 2.0f;

                // set a default background color
                canvas.DrawColor(Color.Black);

                //  canvas.DrawBitmap(_backgroundScaledBitmap, 0, 0, _facePaint);

                // draw the face ticks (if not in ambient mode)
                _facePaint.Alpha = IsInAmbientMode ? 100 : 255;
                var bgToDraw = IsInAmbientMode ? _aodBackgroundScaledBitmap : _backgroundScaledBitmap;
                canvas.DrawBitmap(bgToDraw, 0, 0, _facePaint);

                if (!IsInAmbientMode)
                {

                    // Draw the hour ticks:
                    var hticks = new WatchTicks(_centerX, _centerY, 20, 5, -1) { TickPaint = _tickPaint };
                    hticks.DrawTicks(canvas, 12); // draw 12 of them.

                    // Draw the minute ticks:
                    var mticks = new WatchTicks(_centerX, _centerY, 10, 3, -10) { TickPaint = _minuteTickPaint };
                    mticks.DrawTicks(canvas, 60, 5); // draw 60 of them, but skip every 5th one [60/12 = 5]
                }


                // draw something with the date (change the color based on AOD)
                var dt = _dateTimeAdapter.Now.Date;
                var str = dt.ToString("ddd, dd MMM"); // TUES, 08 APR
                _datePaint.Color = ShouldTimerBeRunning() ? Color.Black : Color.Silver; // (change the color based on AOD)
                _datePaint.TextSize = _centerY / 10.0f;
                var tf = Typeface.Create("Arial", TypefaceStyle.Bold);
                _datePaint.SetTypeface(tf);
                _datePaint.SetShadowLayer(1.5f, -1f, -1f, Color.Argb(100, 50, 50, 50));
                var dateLocation = new Coords(_centerX * 1.10f, _centerY * 1.25f);
                canvas.DrawText(str, dateLocation.X, dateLocation.Y, _datePaint);


                // draw a central hub (bullet hole?)

                var bhW = (int)(width / 4.0f);
                var bhH = (int)(height / 4.0f);
                var bhX = _centerX - bhW / 2.0f;
                var bhY = _centerY - bhH / 2.0f;
                if (_hubScaledBitmap == null)
                    _hubScaledBitmap = Bitmap.CreateScaledBitmap(_hubBitmap, bhW, bhH, true /* filter */);
                canvas.DrawBitmap(_hubScaledBitmap, bhX, bhY, null);

                // set these lengths in the OnSurfaceChanged
                //var minLength = centerX - 40;
                //var hrLength = centerX - 80;


                if (!IsInAmbientMode)
                {
                    // Draw the second hand only in interactive mode:
                    _secHand = new WatchHand(HandType.SECONDS, HandStyle.CENTRIC, _centerX, _centerY, (int)_secLength)
                    {
                        paint = _secondPaint
                    };
                    _secHand.DrawHand(canvas, _calendar);

                    // draw the millisecond tick hand - have to include padding (from outside edge, as neg. in pixels)
                    var milLength = 10;
                    var milPad = -10;
                    _milHand = new WatchHand(HandType.MILLISECONDS, HandStyle.OUTSIDE, _centerX, _centerY, milLength,
                        milPad)
                    { paint = _secondPaint };
                    _milHand.DrawHand(canvas, _calendar);

                    // Draw the minute hand:

                    _minHand = new WatchHand(HandType.MINUTES, HandStyle.CENTRIC, _centerX, _centerY, (int)_minLength)
                    {
                        paint = _minutePaint
                    };
                    _minHand.DrawHand(canvas, _calendar, HAND_END_CAP_RADIUS);

                    // Draw the hour hand:
                    _hrHand = new WatchHand(HandType.HOURS, HandStyle.CENTRIC, _centerX, _centerY, (int)_hrLength)
                    {
                        paint = _hourPaint
                    };
                    _hrHand.DrawHand(canvas, _calendar);
                }
                else
                {
                    // Draw the outline minute hand:
                    var outerMinP = WatchFaceFactory.GetMinuteHand(Color.White, false);
                    outerMinP.StrokeWidth = _minutePaint.StrokeWidth + 4;
                    _minHand = new WatchHand(HandType.MINUTES, HandStyle.CENTRIC, _centerX, _centerY, (int)_minLength)
                    {
                        paint = outerMinP
                    };

                    _minHand.DrawHand(canvas, _calendar);

                    // Draw the outline hour hand:
                    var outerHourP = WatchFaceFactory.GetMinuteHand(Color.White, false);
                    outerHourP.StrokeWidth = _hourPaint.StrokeWidth + 4;
                    _hrHand = new WatchHand(HandType.HOURS, HandStyle.CENTRIC, _centerX, _centerY, (int)_hrLength)
                    {
                        paint = outerHourP
                    };
                    _hrHand.DrawHand(canvas, _calendar);

                    // Draw the minute hand:
                    var innerMinPaint = WatchFaceFactory.GetMinuteHand(Color.Black, false);
                    _minHand = new WatchHand(HandType.MINUTES, HandStyle.CENTRIC, _centerX, _centerY, (int)_minLength)
                    {
                        paint = innerMinPaint
                    };
                    _minHand.DrawHand(canvas, _calendar);

                    // Draw the hour hand:
                    var innerHourP = WatchFaceFactory.GetHourHand(Color.Black, false);
                    _hrHand = new WatchHand(HandType.HOURS, HandStyle.CENTRIC, _centerX, _centerY, (int)_hrLength)
                    {
                        paint = innerHourP
                    };
                    _hrHand.DrawHand(canvas, _calendar);
                }

                // kick off the timer again... maybe
                if (ShouldTimerBeRunning())
                {
                    // recheck the time, and if drawing is taking too long, skip the next tick
                    long delayMs = SystemClock.ElapsedRealtime() - frameStartTimeMs;
                    if (delayMs > INTERACTIVE_UPDATE_RATE_MS)
                    {
                        // This scenario occurs when drawing all of the components takes longer than an actual frame. 
                        // It may be helpful to log how many times this happens, so you can fix it when it occurs.
                        if (Log.IsLoggable(Tag, LogPriority.Warn))
                            Log.Warn(Tag, "OnDraw: long running draw (delayMs=" + delayMs + " > updateRateMs=" + INTERACTIVE_UPDATE_RATE_MS + ")");
                        // In general, you don't want to redraw immediately, but on the next appropriate frame (else block below).
                        delayMs = 0;
                    }
                    else
                    {
                        // Sets the delay as close as possible to the intended framerate.
                        // Note that the recommended interactive update rate is 1 frame per second.
                        // However, if you want to include the sweeping hand gesture, set the
                        // interactive update rate up to 30 frames per second.
                        delayMs = INTERACTIVE_UPDATE_RATE_MS - delayMs;
                    }
                    _mUpdateTimeHandler.SendEmptyMessageDelayed(MSG_UPDATE_TIME, delayMs);
                }
            }

            /// <summary>
            /// Use this to detect that the screen dimensions have changed, and re-scale stuff.
            /// </summary>
            /// <param name="holder"></param>
            /// <param name="format"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            public override void OnSurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
            {
                base.OnSurfaceChanged(holder, format, width, height);

                float mScale = ((float)width) / (float)_backgroundBitmap.Width;

                _backgroundScaledBitmap =
                    Bitmap.CreateScaledBitmap(_backgroundBitmap,
                    (int)(_backgroundBitmap.Width * mScale),
                    (int)(_backgroundBitmap.Height * mScale), true);

                _aodBackgroundScaledBitmap =
                    Bitmap.CreateScaledBitmap(_aodBackgroundBitmap,
                    (int)(_aodBackgroundBitmap.Width * mScale),
                    (int)(_aodBackgroundBitmap.Height * mScale), true);

                // set the lengths of the watch hands
                _hrLength = 0.5f * width / 2;
                _minLength = 0.7f * width / 2;
                _secLength = 0.9f * width / 2;

            }

            public override void OnVisibilityChanged(bool visible)
            {
                base.OnVisibilityChanged(visible);
                // the watch face became visible or invisible 
                if (Log.IsLoggable(Tag, LogPriority.Debug))
                    Log.Debug(Tag, "OnVisibilityChanged: " + visible);
                if (visible)
                {
                    RegisterTimezoneReceiver();

                    // Update time zone in case it changed while we weren't visible.
                    _calendar.TimeZone = Java.Util.TimeZone.Default;
                }
                else
                {
                    UnregisterTimezoneReceiver();
                }

                // Whether the timer should be running depends on whether we're visible and
                // whether we're in ambient mode, so we may need to start or stop the timer
                UpdateTimer();
            }

            // a custom timer for while the watch is in interactive moce
            private void UpdateTimer()
            {

                _mUpdateTimeHandler.RemoveMessages(MSG_UPDATE_TIME);
                if (ShouldTimerBeRunning())
                {
                    _mUpdateTimeHandler.SendEmptyMessage(MSG_UPDATE_TIME);
                }
            }


            /// Run the timer only when visible and in interactive mode:
            private bool ShouldTimerBeRunning()
            {
                return IsVisible && !IsInAmbientMode;
            }

            /// <summary>
            /// Registers the time zone broadcast receiver (defined at the end of this file) to handle time zone change events:
            /// </summary>
            private void RegisterTimezoneReceiver()
            {
                if (_registeredTimezoneReceiver)
                {
                    return;
                }
                _registeredTimezoneReceiver = true;
                IntentFilter filter = new IntentFilter(Intent.ActionTimezoneChanged);
                Application.Context.RegisterReceiver(_timeZoneReceiver, filter);
            }


            /// <summary>
            /// Unregisters the timezone Broadcast receiver:
            /// </summary>
            private void UnregisterTimezoneReceiver()
            {
                if (!_registeredTimezoneReceiver)
                {
                    return;
                }
                _registeredTimezoneReceiver = false;
                Application.Context.UnregisterReceiver(_timeZoneReceiver);
            }



        }
    }
}