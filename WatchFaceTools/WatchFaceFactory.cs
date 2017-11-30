using System;
using Android.Graphics;
using Android.Icu.Text;
using Android.Text.Format;

namespace WatchFaceTools
{

    public static class WatchFaceFactory
    {
        public static float GetHandRotation(HandType type, Time time)
        {
            switch (type)
            {
                case HandType.HOURS:
                    return GetHourHandRotation(time);
                case HandType.MINUTES:
                    return GetMinuteHandRotation(time);
                case HandType.SECONDS:
                    return GetSecondHandRotation(time);
                case HandType.MILLISECONDS:
                    return GetMillisecondHandRotation(time);
                default:
                    return 0.0f;
            }
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

        public static float GetMillisecondHandRotation(Time time)
        {
            SimpleDateFormat formatter = new SimpleDateFormat("SSS");
            var msstr = formatter.Format(time);
            int ms = int.Parse(msstr);
            return ms / 100f * (float)Math.PI;
        }

        /// <summary>
        /// Generates a Paint object of the specified color, with Shadow unless specified shadow = false, and a default StrokeWidth
        /// </summary>
        /// <param name="color"></param>   
        /// /// <param name="shadow"></param>
        /// <param name="pixelWidth"></param>
        /// <returns></returns>
        public static Paint GetHourHand(Color color, bool shadow = true, float pixelWidth = 8.0f)
        {
            var toReturn = getGenericPaint();
            toReturn.Color = color;
            toReturn.StrokeWidth = pixelWidth;
            toReturn.Alpha = 255;
            if (shadow) toReturn.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50));
            return toReturn;
        }

        public static Paint GetHourHand(int a, int r, int g, int b)
        {
            var toReturn = GetHourHand(Color.Black, false);
            toReturn.SetARGB(a, r, g, b);
            return toReturn;
        }

        /// <summary>
        /// Generates a Paint object of the specified color, with Shadow unless specified shadow = false, and a default StrokeWidth
        /// </summary>
        /// <param name="color"></param>
        /// <param name="shadow"></param>
        /// <param name="pixelWidth"></param>
        /// <returns></returns>
        public static Paint GetMinuteHand(Color color, bool shadow = true, float pixelWidth = 5.0f)
        {
            var toReturn = getGenericPaint();
            toReturn.Color = color;
            toReturn.Alpha = 255;
            toReturn.StrokeWidth = pixelWidth;
            if (shadow) toReturn.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50));
            return toReturn;
        }



        public static Paint GetMinuteHand(int a, int r, int g, int b)
        {
            var toReturn = GetMinuteHand(Color.Black, false);
            toReturn.SetARGB(a, r, g, b);
            return toReturn;
        }

        /// <summary>
        /// Generates a Paint object of the specified color, with Shadow unless specified shadow = false, and a default StrokeWidth
        /// </summary>
        /// <param name="color"></param>
        /// <param name="Shadow"></param>
        /// <param name="pixelWidth"></param>
        /// <returns></returns>
        public static Paint GetSecondHand(Color color, bool Shadow = true, float pixelWidth = 3.0f)
        {
            var toReturn = getGenericPaint();
            toReturn.Color = color;
            toReturn.Alpha = 255;
            toReturn.StrokeWidth = pixelWidth;
            if (Shadow) toReturn.SetShadowLayer(0.9f, 2f, 2f, Color.Argb(178, 50, 50, 50));
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



        private static Paint getGenericPaint()
        {
            return new Paint
            {
                AntiAlias = true,
                StrokeCap = Paint.Cap.Round
            };
        }
    }
}
