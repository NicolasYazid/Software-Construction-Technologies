namespace GinRummy.Client.Controls
{
    // Everything one frame of the paint needs is gathered in this single object.
    // The drawing runs on a background thread and must not read any property of the control.
    // The interface thread may be writing those properties at the same time.
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
