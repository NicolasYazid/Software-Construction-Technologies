namespace GinRummy.Client.Controls
{
    // Everything one frame of the paint needs, gathered in a single object so that the drawing
    // runs on a background thread without reading any property of the control while the
    // interface thread may be writing it.
    internal sealed class PaintFrameRequest
    {
        internal byte[] Buffer { get; set; }
        internal int Width { get; set; }
        internal int Height { get; set; }
        internal double ElapsedSeconds { get; set; }
        internal double PatternScale { get; set; }
        internal PaintPalette Palette { get; set; }

        internal PaintGeometry Geometry { get; set; }
    }
}
