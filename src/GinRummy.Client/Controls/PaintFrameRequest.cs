namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Everything one frame of the paint needs, gathered in a single object so that the drawing
    /// runs on a background thread without reading any property of the control while the
    /// interface thread may be writing it.
    /// </summary>
    internal sealed class PaintFrameRequest
    {
        /// <summary>
        /// Gets or sets the buffer the frame is written into.
        /// </summary>
        internal byte[] Buffer { get; set; }

        /// <summary>
        /// Gets or sets the width of the frame in pixels.
        /// </summary>
        internal int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the frame in pixels.
        /// </summary>
        internal int Height { get; set; }

        /// <summary>
        /// Gets or sets the seconds elapsed since the animation started.
        /// </summary>
        internal double ElapsedSeconds { get; set; }

        /// <summary>
        /// Gets or sets how much of the field the frame covers. Above one the surface reaches
        /// further out and the strokes come out smaller.
        /// </summary>
        internal double PatternScale { get; set; }

        /// <summary>
        /// Gets or sets the three colours the frame is painted with.
        /// </summary>
        internal PaintPalette Palette { get; set; }
    }
}
