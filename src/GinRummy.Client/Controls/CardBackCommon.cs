using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Paints once the back of the playing cards: the spiral of the backgrounds, still and in
    /// the reds of the buttons. It is computed at a handful of pixels, so a card stretches it
    /// into the same square blocks the backgrounds show.
    /// </summary>
    public static class CardBackCommon
    {
        private const int SurfaceWidth = 28;
        private const int SurfaceHeight = 40;
        private const int BytesPerPixel = 4;
        private const double StillMoment = 12.0;
        private const double StillPatternScale = 1.0;

        private static readonly Color DeepRed = Color.FromRgb(38, 8, 6);
        private static readonly Color MidRed = Color.FromRgb(139, 44, 31);
        private static readonly Color GlowRed = Color.FromRgb(224, 110, 88);

        private static readonly ImageSource PaintedSurface = PaintSurface();

        /// <summary>
        /// Gets the painted back shared by every card.
        /// </summary>
        public static ImageSource Surface
        {
            get { return PaintedSurface; }
        }

        private static ImageSource PaintSurface()
        {
            PaintFrameRequest request = new PaintFrameRequest
            {
                Buffer = new byte[SurfaceWidth * SurfaceHeight * BytesPerPixel],
                Width = SurfaceWidth,
                Height = SurfaceHeight,
                ElapsedSeconds = StillMoment,
                PatternScale = StillPatternScale,
                Palette = new PaintPalette(DeepRed, MidRed, GlowRed)
            };

            return CtlPaintBackground.PaintStill(request);
        }
    }
}
