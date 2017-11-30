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
        public StartStopCoords(float startX, float startY, float rot, float len)
        {
            var xDiff = (float)Math.Sin(rot) * len;
            var yDiff = (float)-Math.Cos(rot) * len;
            var endX = startX + xDiff;
            var endY = startY + yDiff;
            sPos = new Coords { X = startX, Y = startY };
            ePos = new Coords { X = endX, Y = endY };
        }
    }
}