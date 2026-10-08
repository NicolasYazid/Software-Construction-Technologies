namespace GinRummy.Client.Controls
{
    // The drawing runs on a background thread, and WPF lets only the interface thread read the properties of the control.
    // Everything one frame of the paint needs is therefore copied into this object before the drawing starts.
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
