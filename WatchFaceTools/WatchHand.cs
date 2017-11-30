using System;
using Android.Graphics;
using Android.Text.Format;
using Exception = System.Exception;
using Math = System.Math;

namespace WatchFaceTools
{
    public class WatchHand
    {
        private Time _time;
        private readonly int _centerX;
        private readonly int _centerY;
        private readonly int _length;
        private float _rotation;
        private int _padding;
        private int _width;
        private float _startingRadius = 0.0f;
        private float _endingRadius = 0.0f;

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



        public string name { get; set; }
        private HandType handType { get; }
        public HandStyle HandStyle { get; private set; }
        public Paint paint { get; set; } = new Paint { AntiAlias = true, StrokeCap = Paint.Cap.Round };
        public Coords startCoords { get; set; }


        internal Coords stopCoords { get; set; }

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

        private float GetRotation(Time time)
        {
            _time = time;
            var sec = _time.Second;
            var min = _time.Minute;
            var hr = _time.Hour;
            var ms = DateTime.Now.Millisecond;

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

        public void DrawHand(Canvas canvas, Time time)
        {
            _rotation = GetRotation(time);

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

            //if (!IsStartingAtCenter)
            //    DrawHandAsTick(canvas);
            //else
            //    DrawHandAsLine(canvas);


        }

        private bool _isStartingAtCenter = true;

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