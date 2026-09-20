namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Everything one frame of the liquid background needs in order to be computed. It exists
    /// so that the drawing methods keep their signature within the limit the coding standard
    /// sets, and so that the thread that draws the frame never reads a field of the control
    /// while the interface thread is changing it.
    /// </summary>
    internal sealed class LiquidFrameRequest
    {
        /// <summary>
        /// Gets or sets the buffer the frame is written into, four bytes per pixel.
        /// </summary>
        public byte[] Buffer { get; internal set; }

        /// <summary>
        /// Gets or sets the width of the frame in pixels.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Gets or sets the height of the frame in pixels.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// Gets or sets the time the animation has been running, in seconds.
        /// </summary>
        public double ElapsedSeconds { get; internal set; }

        /// <summary>
        /// Gets or sets the three colours the frame is painted with.
        /// </summary>
        public LiquidPalette Palette { get; internal set; }
    }
}
