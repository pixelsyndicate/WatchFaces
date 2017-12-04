using System;
using Android.Graphics;
using Android.Text.Format;
using Java.Util;
using Exception = System.Exception;
using Math = System.Math;

namespace WatchFaceTools
{
    public class WatchHand
    {
        private Date _time;
        private readonly int _centerX;
        private readonly int _centerY;
        private readonly int _length;
        private float _rotation;
        private int _padding;
        private int _width;
        private readonly float _startingRadius = 0.0f;
        private readonly float _endingRadius = 0.0f;

        public float GetRotation()
        {
            return _rotation;
        }
        public WatchHand(HandType type, HandStyle style, float centerX, float centerY, int length)
        {
            handType = type;
            HandStyle = style;
            _centerX = (int)centerX;
            _centerY = (int)centerY;
            _length = (int)length;
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
            _centerX = (int)centerX;
            _centerY = (int)centerY;
            _length = (int)length;
            if (string.IsNullOrEmpty(name))
                name = type.ToString();
            _isStartingAtCenter = style == HandStyle.CENTRIC;
            _padding = padding;
            _startingRadius = centerX + padding - length;
            _endingRadius = centerX + padding;
        }


        private string name { get; set; }
        private HandType handType { get; }
        private HandStyle HandStyle { get; set; }
        public Paint paint { get; set; } = new Paint { AntiAlias = true, StrokeCap = Paint.Cap.Round };
        private Coords startCoords { get; set; }


        private Coords stopCoords { get; set; }

        public float width
        {
            get => paint.StrokeWidth <= 0.0f ? 1f : paint.StrokeWidth;
            set => paint.StrokeWidth = value;
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
        /// Returns a float value for the rotation
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
                    return ms / 500f * (float)Math.PI;
                case HandType.SECONDS:
                    return sec / 30f * (float)Math.PI;
                case HandType.MINUTES:
                    return min / 30f * (float)Math.PI;
                case HandType.HOURS:
                    return (hr + min / 60f) / 6f * (float)Math.PI;
                default:
                    throw new Exception("WatchHand not valid.");
            }
        }

        public void DrawHand(Canvas canvas, Calendar calendar)
        {
            _rotation = GetRotation(calendar);

            if (!_isStartingAtCenter)
            {
                var innerX = (float)Math.Sin(_rotation) * _startingRadius;
                var innerY = (float)-Math.Cos(_rotation) * _startingRadius;
                var outerX = (float)Math.Sin(_rotation) * _endingRadius;
                var outerY = (float)-Math.Cos(_rotation) * _endingRadius;
                startCoords = new Coords { X = _centerX + innerX, Y = _centerY + innerY };
                stopCoords = new Coords { X = _centerX + outerX, Y = _centerY + outerY };
                var coords = new StartStopCoords(startCoords, stopCoords);
            }
            else
            {
                var coords = new StartStopCoords(_centerX, _centerY, _rotation, _length);
                startCoords = coords.sPos;
                stopCoords = coords.ePos;
            }

            canvas.DrawLine(startCoords.X, startCoords.Y, stopCoords.X, stopCoords.Y, paint);
        }

        public void DrawHand(Canvas canvas, Calendar calendar, float endcapRadius)
        {
            _rotation = GetRotation(calendar);
            StartStopCoords coords;
            if (!_isStartingAtCenter)
            {
                var innerX = (float)Math.Sin(_rotation) * _startingRadius;
                var innerY = (float)-Math.Cos(_rotation) * _startingRadius;
                var outerX = (float)Math.Sin(_rotation) * _endingRadius;
                var outerY = (float)-Math.Cos(_rotation) * _endingRadius;
                startCoords = new Coords { X = _centerX + innerX, Y = _centerY + innerY };
                stopCoords = new Coords { X = _centerX + outerX, Y = _centerY + outerY };
                coords = new StartStopCoords(startCoords, stopCoords);
            }
            else
            {
                coords = new StartStopCoords(_centerX, _centerY, _rotation, _length, endcapRadius);

                startCoords = coords.sPos;
                stopCoords = coords.ePos;
            }



            canvas.DrawLine(startCoords.X, startCoords.Y, stopCoords.X, stopCoords.Y, paint);

        }

        private const double TOLERANCE = 0.0;

        //private void DrawHandAsRoundedRect(Canvas canvas, float handLength)
        //{
        //    canvas.DrawRoundRect(mCenterX - HAND_END_CAP_RADIUS,
        //        mCenterY - handLength, mCenterX + HAND_END_CAP_RADIUS,
        //        mCenterY + HAND_END_CAP_RADIUS, HAND_END_CAP_RADIUS,
        //        HAND_END_CAP_RADIUS, mHandPaint);
        //}
        private readonly bool _isStartingAtCenter;

        private void DrawHandAsTick(Canvas canvas)
        {
            var innerX = (float)Math.Sin(_rotation) * _startingRadius;
            var innerY = (float)-Math.Cos(_rotation) * _startingRadius;
            var outerX = (float)Math.Sin(_rotation) * _endingRadius;
            var outerY = (float)-Math.Cos(_rotation) * _endingRadius;

            startCoords = new Coords { X = _centerX + innerX, Y = _centerY + innerY };
            stopCoords = new Coords { X = _centerX + outerX, Y = _centerY + outerY };

            canvas.DrawLine(startCoords.X, startCoords.Y, stopCoords.X, stopCoords.Y, paint);
        }

        private void DrawHandAsLine(Canvas canvas)
        {
            var coords = new StartStopCoords(_centerX, _centerY, _rotation, _length);
            startCoords = coords.sPos;
            stopCoords = coords.ePos;
            canvas.DrawLine(startCoords.X, startCoords.Y, stopCoords.X, stopCoords.Y, paint);
        }
    }
}