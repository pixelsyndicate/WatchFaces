using System;
using System.IO;
using Android.Graphics;
using Android.Text.Format;

namespace WatchFace
{
    public static class WatchFaceFactory
    {

        public static Paint GetHourHand(Color color, bool Shadow = true)
        {
            var toReturn = getGenericPaint();
            toReturn.Color = color;
            toReturn.StrokeWidth = 8.0f;

            if (Shadow)
                toReturn.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50));
            return toReturn;
        }

        public static Paint GetHourHand(int a, int r, int g, int b)
        {
            var toReturn = GetHourHand(Color.Black, false);
            toReturn.SetARGB(a, r, g, b);
            return toReturn;
        }

        public static Paint GetMinuteHand(Color color, bool Shadow = true)
        {
            var toReturn = getGenericPaint();
            toReturn.Color = color;
            toReturn.StrokeWidth = 5.0f;
            if (Shadow)
                toReturn.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50));
            return toReturn;
        }

        private static Paint getGenericPaint()
        {
            return new Paint
            {
                AntiAlias = true,
                StrokeCap = Paint.Cap.Round
            };
        }

        public static Paint GetMinuteHand(int a, int r, int g, int b)
        {
            var toReturn = GetMinuteHand(Color.Black, false);
            toReturn.SetARGB(a, r, g, b);
            return toReturn;
        }

        public static Paint GetSecondHand(Color color, bool Shadow = true)
        {
            var toReturn = getGenericPaint();
            toReturn.Color = color;
            toReturn.StrokeWidth = 3.0f;

            if (Shadow)
                toReturn.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50));
            return toReturn;
        }

        public static Paint GetSecondHand(int a, int r, int g, int b)
        {
            var toReturn = GetSecondHand(Color.Black, false);
            toReturn.SetARGB(a, r, g, b);
            return toReturn;
        }

        public static StartStopCoords GetDrawLineStartAndStops(float centerX, float centerY, float rotation, float length)
        {
            //  var testToReturn = new StartStopCoords(centerX, centerY, rotation, length);
            var xDiff = (float)Math.Sin(rotation) * length;
            var yDiff = (float)-Math.Cos(rotation) * length;
            var endX = centerX + xDiff;
            var endY = centerY + yDiff;
            var startAt = new Coords { X = centerX, Y = centerY };
            var endAt = new Coords { X = endX, Y = endY };
            return new StartStopCoords(startAt, endAt);
        }

        /// <summary>
        /// assuming the clock face is circular, and 12 is top-center, 
        /// then pass in the TIME object and the angle will be returned for the Time.Second, with 30 divisions
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float GetSecondHandRotation(Time time)
        {
            return time.Second / 30f * (float)Math.PI;
        }

        /// <summary>
        /// assuming the clock face is circular, and 12 is top-center, 
        /// then pass in the TIME object and the angle will be returned for the Time.Minute, with 30 divisions
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float GetMinuteHandRotation(Time time)
        {
            return time.Minute / 30f * (float)Math.PI;
        }

        /// <summary>
        /// assuming the clock face is circular, and 12 is top-center, 
        /// then pass in the TIME object and the angle will be returned for the Time.Hour, with 12 divisions
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float GetHourHandRotation(Time time)
        {
            return (time.Hour + time.Minute / 60f) / 6f * (float)Math.PI;
        }

        //public class MyOldWatchFaceService : CanvasWatchFaceService
        //{
        //    // This Paint object will be used to draw the current time on the watch face.
        //    private static Paint hoursPaint;

        //    public override WallpaperService.Engine OnCreateEngine()
        //    {
        //        return new MyOldWatchFaceEngine(this);
        //    }

        //    public class MyOldWatchFaceEngine : CanvasWatchFaceService.Engine
        //    {
        //        CanvasWatchFaceService owner;

        //        public MyOldWatchFaceEngine(CanvasWatchFaceService owner) : base(owner)
        //        {
        //            this.owner = owner;
        //        }

        //        public override void OnCreate(ISurfaceHolder holder)
        //        {
        //            base.OnCreate(holder);

        //            SetWatchFaceStyle(new WatchFaceStyle.Builder(owner)
        //                .SetCardPeekMode(WatchFaceStyle
        //                    .PeekModeShort) // Sets peek mode to PeekModeShort, which causes notifications to appear as small "peek" cards on the display.
        //                .SetBackgroundVisibility(WatchFaceStyle
        //                    .BackgroundVisibilityInterruptive) // Sets the background visibility to Interruptive, which causes the background of a peek card to be shown only briefly if it represents an interruptive notification.
        //                .SetShowSystemUiTime(
        //                    false) // Disables the default system UI time from being drawn on the watch face so that the custom watch face can display the time instead.
        //                .Build());

        //            // After SetWatchFaceStyle completes, OnCreate instantiates the Paint object (hoursPaint) and sets its color to white and its text size to 48 pixels (TextSize must be specified in pixels).
        //            hoursPaint = new Paint
        //            {
        //                Color = Color.White,
        //                TextSize = 48f, AntiAlias = true,
        //            };
        //        }

        //        // the method that actually draws watch face elements such as digits and clock face hands. In the following example, it draws a time string on the watch face.
        //        public override void OnDraw(Canvas canvas, Rect frame)
        //        {
        //            var str = DateTime.Now.ToString("h:mm tt");
        //            canvas.DrawText(str,
        //                (float) (frame.Left + 70),
        //                (float) (frame.Top + 80), hoursPaint);
        //        }

        //        // Android periodically calls the OnTimeTick method to update the time shown by the watch face. It is called at least once per minute (in both ambient and interactive modes), or when the date/time or timezone have changed
        //        public override void OnTimeTick()
        //        {
        //            Invalidate(); // This implementation of OnTimeTick simply calls Invalidate. The Invalidate method schedules OnDraw to redraw the watch face.
        //        }
        //    }
        //}
    }
}