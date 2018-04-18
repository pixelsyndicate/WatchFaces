using System;

namespace WatchFaceTools
{
    public class Coords
    {
        public Coords(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Coords()
        {

        }

        public float X { get; set; }
        public float Y { get; set; }

        public static Coords GetOffset(float rot, float rad)
        {
            var x = (float)Math.Sin(rot) * rad;
            var y = (float)-Math.Cos(rot) * rad;
            return new Coords(x, y);
        }
    }

    
}