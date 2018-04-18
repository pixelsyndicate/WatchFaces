using System;

namespace WatchFaceTools
{
    public class StartStopCoords
    {
        public Coords sPos { get; private set; }
        public Coords ePos { get; private set; }

        /// <summary>
        /// Pass in starting coordinates and ending coordinates
        /// </summary>
        /// <param name="startPos">Starting Position (x,y)</param>
        /// <param name="endPos">Ending Position (x,y)</param>
        public StartStopCoords(Coords startPos, Coords endPos)
        {
            this.sPos = startPos;
            this.ePos = endPos;
        }

        /// <summary>
        /// Pass in the X, Y of the center of the screen (or starting position of the hand)
        /// and the Rotation angle and the Length
        /// </summary>
        /// <param name="startX">X position to start from</param>
        /// <param name="startY">Y position to start from</param>
        /// <param name="rot">angle of rotation</param>
        /// <param name="len">length of the distance between start and stop</param>
        /// <param name="endcapRad">radius of the endcap of your </param>
        public StartStopCoords(float startX, float startY, float rot, float len, float endcapRad = 0.0f)
        {
            len += endcapRad;
            var xDiff = (float)Math.Sin(rot) * len;
            var yDiff = (float)-Math.Cos(rot) * len;
            startX -= endcapRad;
            startY -= endcapRad;
            var endX = startX + xDiff;
            var endY = startY + yDiff;
            sPos = new Coords { X = startX, Y = startY };
            ePos = new Coords { X = endX, Y = endY };
        }



        public static StartStopCoords GetOffsetCenterCoords(int centerX, int centerY, float rotation, float startingRadius, float endingRadius)
        {
            // coordinates for start position
            Coords offsetInner = Coords.GetOffset(rotation, startingRadius);
            //var innerX = (float)Math.Sin(rotation) * startingRadius;
            //var innerY = (float)-Math.Cos(rotation) * startingRadius;

            // coordinates for end position.
            Coords offsetOuter = Coords.GetOffset(rotation, endingRadius);
            //var outerX = (float)Math.Sin(rotation) * endingRadius;
            //var outerY = (float)-Math.Cos(rotation) * endingRadius;

            var startC = new Coords(centerX + offsetInner.X, centerY + offsetInner.Y);// new Coords { X = centerX + innerX, Y = centerY + innerY };
            var endC = new Coords(centerX + offsetOuter.X, centerY + offsetOuter.Y);// new Coords { X = centerX + outerX, Y = centerY + outerY };

            var toReturn = new StartStopCoords(startC, endC);
            return toReturn;
        }


        public static StartStopCoords GetCenterCoords(int centerX, int centerY, float rotation, int length)
        {
            return new StartStopCoords(centerX, centerY, rotation, length);
        }
    }
}