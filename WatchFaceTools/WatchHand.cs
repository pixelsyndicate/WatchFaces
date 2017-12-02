using System;
using Android.Graphics;
using Android.Text.Format;
using Java.Util;
using Exception = System.Exception;
using Math = System.Math;

namespace WatchFaceTools
{
    public class WatchHand : ScreenElement
    {
        private readonly int _centerX;
        private readonly int _centerY;
        private readonly float _startingRadius = 0.0f;
        private readonly float _endingRadius = 0.0f;
        private HandType handType { get; }
        private HandStyle HandStyle { get; set; }
        protected internal override float Width
        {
            get => Paint.StrokeWidth <= 0.0f ? 1f : Paint.StrokeWidth;
            set => Paint.StrokeWidth = value;
        }

        public WatchHand(HandType type, HandStyle style, float centerX, float centerY, int length)
        {
            Paint = new Paint {AntiAlias = true, StrokeCap = Paint.Cap.Round};
            handType = type;
            HandStyle = style;
            _centerX = (int) centerX;
            _centerY = (int) centerY;
            Length = (int) length;
            if (string.IsNullOrEmpty(Name))
                Name = type.ToString();
            _isStartingAtCenter = style == HandStyle.CENTRIC;
            _startingRadius = centerX;
            _endingRadius = centerX;
        }

        public WatchHand(HandType type, HandStyle style, float centerX, float centerY, int length, int padding)
        {
            Paint = new Paint {AntiAlias = true, StrokeCap = Paint.Cap.Round};
            handType = type;
            HandStyle = style;
            _centerX = (int) centerX;
            _centerY = (int) centerY;
            Length = (int) length;
            if (string.IsNullOrEmpty(Name))
                Name = type.ToString();
            _isStartingAtCenter = style == HandStyle.CENTRIC;
            Padding = padding;
            _startingRadius = centerX + padding - length;
            _endingRadius = centerX + padding;
        }

        private readonly bool _isStartingAtCenter;

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
            Rotation = GetRotation(calendar);
            StartStopCoords coords;
            if (!_isStartingAtCenter)
            {
                var innerX = (float) Math.Sin(Rotation) * _startingRadius;
                var innerY = (float) -Math.Cos(Rotation) * _startingRadius;
                var outerX = (float) Math.Sin(Rotation) * _endingRadius;
                var outerY = (float) -Math.Cos(Rotation) * _endingRadius;
                coords = new StartStopCoords(
                    new Coords {X = _centerX + innerX, Y = _centerY + innerY},
                    new Coords {X = _centerX + outerX, Y = _centerY + outerY});
            }
            else
            {
                coords = new StartStopCoords(_centerX, _centerY, Rotation, Length);
            }

            canvas.DrawLine(coords.sPos.X, coords.sPos.Y, coords.ePos.X, coords.ePos.Y, Paint);
        }

        private void DrawHandAsTick(Canvas canvas)
        {
            var innerX = (float) Math.Sin(Rotation) * _startingRadius;
            var innerY = (float) -Math.Cos(Rotation) * _startingRadius;
            var outerX = (float) Math.Sin(Rotation) * _endingRadius;
            var outerY = (float) -Math.Cos(Rotation) * _endingRadius;
            var coords = new StartStopCoords(
                new Coords {X = _centerX + innerX, Y = _centerY + innerY},
                new Coords {X = _centerX + outerX, Y = _centerY + outerY});

            canvas.DrawLine(coords.sPos.X, coords.sPos.Y, coords.ePos.X, coords.ePos.Y, Paint);
        }

        private void DrawHandAsLine(Canvas canvas)
        {
            var coords = new StartStopCoords(_centerX, _centerY, Rotation, Length);
            canvas.DrawLine(coords.sPos.X, coords.sPos.Y, coords.ePos.X, coords.ePos.Y, Paint);
        }
    }
}