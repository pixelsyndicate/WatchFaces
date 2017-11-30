using System;
using System.Collections.Generic;
using Android.Graphics;

namespace WatchFaceTools
{
    public class WatchTicks
    {
        private readonly float _centerX;
        private readonly float _centerY;
        private int _number;
        private int _skipEveryNth;

        /// <summary>
        ///   Use this to create an object that can create a series watchface ticks around a central point.
        ///   Call the method DrawTicks() to draw the ticks onto the canvas.
        /// </summary>
        /// <param name="centerX">X coordinates for the center point of the ticks</param>
        /// <param name="centerY">Y coordinates for the center point of the ticks</param>
        /// <param name="length">length (in pixels) to make this series of ticks</param>
        /// <param name="width">length (in pixels) to make this series of ticks</param>
        /// <param name="padding">distance (in +/- pixels) from the screen edge to pad (DEFAULT: -5)</param>
        public WatchTicks(float centerX, float centerY, int length, int width, int padding = -5)
        {
            _centerX = centerX;
            _centerY = centerY;
            Length = length;
            Padding = padding;
            Width = width;
            StartingRadius = centerX + padding - length;
            EndingRadius = centerX + padding;
            TickPaint = new Paint {StrokeWidth = width, AntiAlias = true};
        }
        
        private int Length { get; }

        private int Width { get; }

        private int Padding { get; }

        private float StartingRadius { get; }

        private float EndingRadius { get; }

        public Paint TickPaint { get; set; }
        
        private List<StartStopCoords> StartStopCoords { get; } = new List<StartStopCoords>();

        private StartStopCoords[] GetTickCoords(int number)
        {
            _number = number;
            // populate tick collection
            for (var tickIndex = 0; tickIndex < _number; tickIndex++)
            {
                var tickRot = (float) (tickIndex * Math.PI * 2 / _number);
                var innerX = (float) Math.Sin(tickRot) * StartingRadius;
                var innerY = (float) -Math.Cos(tickRot) * StartingRadius;
                var outerX = (float) Math.Sin(tickRot) * EndingRadius;
                var outerY = (float) -Math.Cos(tickRot) * EndingRadius;
                StartStopCoords.Add(new StartStopCoords(
                    new Coords {X = _centerX + innerX, Y = _centerY + innerY},
                    new Coords {X = _centerX + outerX, Y = _centerY + outerY}));
            }
            return StartStopCoords.ToArray();
        }

        private StartStopCoords[] GetTickCoords(int number, int skipEvery)
        {
            _number = number;
            _skipEveryNth = skipEvery;
            // populate tick collection
            for (var tickIndex = 0; tickIndex < _number; tickIndex++)
            {
                // todo: don't draw it if it's on an HOUR mark... did that already.
                if (tickIndex % _skipEveryNth == 0)
                    continue;
                var tickRot = (float) (tickIndex * Math.PI * 2 / _number);
                var innerX = (float) Math.Sin(tickRot) * StartingRadius;
                var innerY = (float) -Math.Cos(tickRot) * StartingRadius;
                var outerX = (float) Math.Sin(tickRot) * EndingRadius;
                var outerY = (float) -Math.Cos(tickRot) * EndingRadius;
                StartStopCoords.Add(new StartStopCoords(
                    new Coords {X = _centerX + innerX, Y = _centerY + innerY},
                    new Coords {X = _centerX + outerX, Y = _centerY + outerY}));
            }
            return StartStopCoords.ToArray();
        }

        /// <summary>
        ///   Call this method to draw the specified number of ticks on a specified canvas.
        ///   Use the overloaded method to have the option to skip every nth one.
        /// </summary>
        /// <param name="canvas">The object you will want to have these ticks.</param>
        /// <param name="number">The number of ticks to be drawn out (evenly spaced)</param>
        public void DrawTicks(Canvas canvas, int number)
        {
            foreach (var ssc in GetTickCoords(number))
                canvas.DrawLine(ssc.sPos.X, ssc.sPos.Y, ssc.ePos.X, ssc.ePos.Y, TickPaint);
        }

        /// <summary>
        ///   Call this method to draw the specified number of ticks on a specified canvas, with the option to skip every nth one.
        /// </summary>
        /// <param name="canvas">The object you will want to have these ticks.</param>
        /// <param name="number">The number of ticks to be drawn out (evenly spaced)</param>
        /// <param name="skipevery">
        ///   a number in which to skip every Nth. Example: to not draw every 5th tick, enter 5 here. This is
        ///   used to not overwrite other ticks that may be in place.
        /// </param>
        public void DrawTicks(Canvas canvas, int number, int skipevery)
        {
            foreach (var ssc in GetTickCoords(number, skipevery))
                canvas.DrawLine(ssc.sPos.X, ssc.sPos.Y, ssc.ePos.X, ssc.ePos.Y, TickPaint);
        }
    }
}