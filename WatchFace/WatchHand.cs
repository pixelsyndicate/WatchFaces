﻿using System;
using Android.Graphics;
using Android.Text.Format;

namespace WatchFace
{
    public class WatchHand
    {
        private Time _time;
        private int _centerX;
        private int _centerY;
        private int _length;
        private float _rotation;
        public WatchHand(HandType type, float centerX, float centerY, float length)
        {
            handType = type;
            _centerX = (int)centerX;
            _centerY = (int)centerY;
            _length = (int)length;
            if (name == string.Empty)
                name = type.ToString();
        }

        public string name { get; set; }
        private HandType handType { get; }
        public Paint paint { get; set; } = new Paint { AntiAlias = true, StrokeCap = Paint.Cap.Round };
        public Coords startCoords { get; set; }
  

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

        private float GetRotation(Time time)
        {
            var toReturn = 0.0f;
            _time = time;
            var sec = _time.Second;
            var min = _time.Minute;
            var hr = _time.Hour;
            var ms = _time.ToMillis(true /*isgnorDist*/);
            switch (handType)
            {
                case HandType.SECONDS:
                    toReturn = sec / 30f * (float)Math.PI;
                    break;
                case HandType.MINUTES:
                    toReturn = min / 30f * (float)Math.PI;
                    break;
                case HandType.HOURS:
                    toReturn = (hr + min / 60f) / 6f * (float)Math.PI;
                    break;
                case HandType.MILLISECONDS:
                    toReturn = ms / 100f * (float)Math.PI;
                    break;
            }
            return toReturn;
        }

        public void DrawHand(Canvas canvas, Time time)
        {
            _time = time;
            _rotation = GetRotation(_time);
            var coords = new StartStopCoords(_centerX, _centerY, _rotation, _length);
            startCoords = coords.sPos;
            stopCoords = coords.ePos;
            canvas.DrawLine(startCoords.X, startCoords.Y, stopCoords.X, stopCoords.Y, paint);
        }
    }
}