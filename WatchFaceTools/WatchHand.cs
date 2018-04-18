using System;
using Android.Graphics;
using Java.Util;

namespace WatchFaceTools
{
    public class WatchHand
    {
        private const double TOLERANCE = 0.0;
        private readonly int _centerX;
        private readonly int _centerY;
        private readonly float _endingRadius;


        private readonly bool _isStartingAtCenter;
        private readonly int _length;
        private readonly float _startingRadius;
        private int _padding;
        private float _rotation;
        private Date _time;
        private int _width;

        public WatchHand(HandType type, HandStyle style, float centerX, float centerY, int length)
        {
            handType = type;
            HandStyle = style;
            _centerX = (int) centerX;
            _centerY = (int) centerY;
            _length = length;
            if (string.IsNullOrEmpty(name))
                name = type.ToString();
            _isStartingAtCenter = style == HandStyle.CENTRIC;
            _startingRadius = centerX;
            _endingRadius = centerX;
        }

        public WatchHand(HandType type, HandStyle style, float centerX, float centerY, int length, int padding)
        {
            handType = type;
            HandStyle = style;
            _centerX = (int) centerX;
            _centerY = (int) centerY;
            _length = length;
            if (string.IsNullOrEmpty(name))
                name = type.ToString();
            _isStartingAtCenter = style == HandStyle.CENTRIC;
            _padding = padding;
            _startingRadius = centerX + padding - length;
            _endingRadius = centerX + padding;
        }


        private string name { get; }
        private HandType handType { get; }
        private HandStyle HandStyle { get; }

        public Paint paint { get; set; } = new Paint {AntiAlias = true, StrokeCap = Paint.Cap.Round};
        //private Coords startCoords { get; set; }


        //private Coords stopCoords { get; set; }

        public float width
        {
            get => paint.StrokeWidth <= 0.0f ? 1f : paint.StrokeWidth;
            set => paint.StrokeWidth = value;
        }

        public float GetRotation()
        {
            return _rotation;
        }

        public void SetAlpha(int a)
        {
            paint.Alpha = a;
        }

        public void SetColor(Color c)
        {
            paint.Color = c;
        }

        /// <summary>
        ///   Returns a float value for the rotation
        /// </summary>
        /// <param name="calendar"></param>
        /// <returns></returns>
        private float GetRotation(Calendar calendar)
        {
            var sec = calendar.Get(CalendarField.Second);
            var min = calendar.Get(CalendarField.Minute);
            var hr = calendar.Get(CalendarField.Hour);
            var ms = calendar.Get(CalendarField.Millisecond);

            switch (handType)
            {
                case HandType.MILLISECONDS:
                    return ms / 500f * (float) Math.PI;
                case HandType.SECONDS:
                    return sec / 30f * (float) Math.PI;
                case HandType.MINUTES:
                    return min / 30f * (float) Math.PI;
                case HandType.HOURS:
                    return (hr + min / 60f) / 6f * (float) Math.PI;
                default:
                    throw new Exception("WatchHand not valid.");
            }
        }

        public void DrawHand(Canvas canvas, Calendar calendar)
        {
            _rotation = GetRotation(calendar);
            StartStopCoords coords = null;

            if (!_isStartingAtCenter)
                coords = StartStopCoords.GetOffsetCenterCoords(_centerX, _centerY, _rotation, _startingRadius,
                    _endingRadius);
            else
                coords = StartStopCoords.GetCenterCoords(_centerX, _centerY, _rotation, _length);

            canvas.DrawLine(coords.sPos.X, coords.sPos.Y, coords.ePos.X, coords.ePos.Y, paint);
        }

        public void DrawHand(Canvas canvas, Calendar calendar, float endcapRadius)
        {
            _rotation = GetRotation(calendar);
            StartStopCoords coords;
            if (!_isStartingAtCenter)
                coords = StartStopCoords.GetOffsetCenterCoords(_centerX, _centerY, _rotation, _startingRadius+endcapRadius,
                    _endingRadius-endcapRadius);
            else
                coords = StartStopCoords.GetCenterCoords(_centerX, _centerY, _rotation, _length);

            canvas.DrawLine(coords.sPos.X, coords.sPos.Y, coords.ePos.X, coords.ePos.Y, paint);
        }

        private void DrawHandAsTick(Canvas canvas)
        {
            var coords =
                StartStopCoords.GetOffsetCenterCoords(_centerX, _centerY, _rotation, _startingRadius, _endingRadius);
            canvas.DrawLine(coords.sPos.X, coords.sPos.Y, coords.ePos.X, coords.ePos.Y, paint);
        }

        private void DrawHandAsLine(Canvas canvas)
        {
            var coords = StartStopCoords.GetCenterCoords(_centerX, _centerY, _rotation, _length);

            canvas.DrawLine(coords.sPos.X, coords.sPos.Y, coords.ePos.X, coords.ePos.Y, paint);
        }
    }
}