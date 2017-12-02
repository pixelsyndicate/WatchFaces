using Android.Graphics;

namespace WatchFaceTools
{
    public abstract class ScreenElement
    {
        /// <summary>
        /// Width is being applied to the Paint StrokeWidth
        /// </summary>
        protected internal abstract float Width { get; set; }
        protected internal int Length { get; set; }
        protected internal int Padding { get; set; }
        protected internal string Name { get; set; }
        protected internal float Rotation { get; set; }

        public Paint Paint { get; set; }
        public void SetAlpha(int a)
        {
            Paint.Alpha = a;
        }

        public void SetColor(Color c)
        {
            Paint.Color = c;
        }
    }
}