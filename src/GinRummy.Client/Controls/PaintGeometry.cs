namespace GinRummy.Client.Controls
{
    // The part of the paint that depends only on where a pixel sits and not on the moment:
    // its distance to the centre, the angle it starts turning from and the shade of the frame.
    // It is measured once for each size of surface, so every frame only adds the movement.
    // Once built it is only read, so the drawing threads can share it.
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
