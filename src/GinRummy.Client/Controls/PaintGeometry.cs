namespace GinRummy.Client.Controls
{
    // What depends only on where a pixel sits is measured once for each size of surface, so every frame only adds the movement.
    // The geometry must stay read-only once built, because the rows of a frame are drawn in parallel and share it.
    internal sealed class PaintGeometry
    {
        internal PaintGeometry(int pixelCount, double patternScale)
        {
            PatternScale = patternScale;
            Reaches = new double[pixelCount];
            Turns = new double[pixelCount];
            Shades = new double[pixelCount];
        }

        internal double PatternScale { get; private set; }
        internal double[] Reaches { get; private set; }
        internal double[] Turns { get; private set; }
        internal double[] Shades { get; private set; }
    }
}
