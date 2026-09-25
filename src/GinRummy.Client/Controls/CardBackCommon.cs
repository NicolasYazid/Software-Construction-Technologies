using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Paints once the sprite of the back of the playing cards: the spiral of the backgrounds,
    /// still and in the reds of the buttons. The blocks are enlarged here, one by one, well past
    /// the size of any card, so every card only ever shrinks the sprite. Shrinking keeps the
    /// edges of the blocks sharp at any size of card and any scale of screen, while stretching
    /// a small surface is what blurred them.
    /// </summary>
    public static class CardBackCommon
    {
        private const int BlockSize = 4;
        private const int BytesPerPixel = 4;
        private const double DotsPerInch = 96.0;
        private const int Columns = 60;
        private const int Rows = 84;
        private const double StillMoment = 12.0;
        private const double StillPatternScale = 1.4;

        private static readonly Color DeepRed = Color.FromRgb(38, 8, 6);
        private static readonly Color MidRed = Color.FromRgb(139, 44, 31);
        private static readonly Color GlowRed = Color.FromRgb(224, 110, 88);

        private static readonly ImageSource PaintedSprite = PaintSprite();

        /// <summary>
        /// Gets the sprite of the back shared by every card, whatever its size.
        /// </summary>
        public static ImageSource Sprite
        {
            get { return PaintedSprite; }
        }

        private static ImageSource PaintSprite()
        {
            PaintFrameRequest request = new PaintFrameRequest
            {
                Buffer = new byte[Columns * Rows * BytesPerPixel],
                Width = Columns,
                Height = Rows,
                ElapsedSeconds = StillMoment,
                PatternScale = StillPatternScale,
                Palette = new PaintPalette(DeepRed, MidRed, GlowRed)
            };

            CtlPaintBackground.PaintStillFrame(request);

            return Enlarge(request);
        }

        private static BitmapSource Enlarge(PaintFrameRequest request)
        {
            int width = request.Width * BlockSize;
            int height = request.Height * BlockSize;
            int stride = width * BytesPerPixel;
            byte[] pixels = new byte[stride * height];

            for (int rowIndex = 0; rowIndex < height; rowIndex++)
            {
                int sourceRow = (rowIndex / BlockSize) * request.Width * BytesPerPixel;
                for (int columnIndex = 0; columnIndex < width; columnIndex++)
                {
                    int source = sourceRow + ((columnIndex / BlockSize) * BytesPerPixel);
                    int target = (rowIndex * stride) + (columnIndex * BytesPerPixel);
                    Array.Copy(request.Buffer, source, pixels, target, BytesPerPixel);
                }
            }

            BitmapSource enlarged = BitmapSource.Create(
                width,
                height,
                DotsPerInch,
                DotsPerInch,
                PixelFormats.Pbgra32,
                null,
                pixels,
                stride);
            enlarged.Freeze();

            return enlarged;
        }
    }
}
