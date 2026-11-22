using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GinRummy.Client.Controls
{
    // The blocks are enlarged here well past the size of any card, so every card only ever shrinks the sprite.
    // Shrinking keeps the edges of the blocks sharp at any size of card and any scale of screen.
    // Stretching a small surface is what blurred them before.
    public static class CardBackCommon
    {
        private const int BlockSize = 4;
        private const int Columns = 60;
        private const int Rows = 84;
        private const double StillMoment = 12.0;
        private const double StillPatternScale = 1.4;

        private static readonly Color s_deepRed = Color.FromRgb(38, 8, 6);
        private static readonly Color s_midRed = Color.FromRgb(139, 44, 31);
        private static readonly Color s_glowRed = Color.FromRgb(224, 110, 88);

        private static readonly ImageSource s_paintedSprite = PaintSprite();

        public static ImageSource Sprite
        {
            get { return s_paintedSprite; }
        }

        private static ImageSource PaintSprite()
        {
            PaintFrameRequest request = new PaintFrameRequest
            {
                Buffer = new byte[Columns * Rows * PaintedBackground.BytesPerPixel],
                Width = Columns,
                Height = Rows,
                ElapsedSeconds = StillMoment,
                PatternScale = StillPatternScale,
                Palette = new PaintPalette(s_deepRed, s_midRed, s_glowRed)
            };

            PaintedBackground.PaintStillFrame(request);

            return Enlarge(request);
        }

        private static BitmapSource Enlarge(PaintFrameRequest request)
        {
            int width = request.Width * BlockSize;
            int height = request.Height * BlockSize;
            int stride = width * PaintedBackground.BytesPerPixel;
            byte[] pixels = new byte[stride * height];

            for (int rowIndex = 0; rowIndex < height; rowIndex++)
            {
                int sourceRow = (rowIndex / BlockSize) * request.Width * PaintedBackground.BytesPerPixel;
                for (int columnIndex = 0; columnIndex < width; columnIndex++)
                {
                    int source = sourceRow + ((columnIndex / BlockSize) * PaintedBackground.BytesPerPixel);
                    int target = (rowIndex * stride) + (columnIndex * PaintedBackground.BytesPerPixel);
                    Array.Copy(request.Buffer, source, pixels, target, PaintedBackground.BytesPerPixel);
                }
            }

            BitmapSource enlarged = BitmapSource.Create(
                width,
                height,
                PaintedBackground.BitmapDotsPerInch,
                PaintedBackground.BitmapDotsPerInch,
                PixelFormats.Pbgra32,
                null,
                pixels,
                stride);
            enlarged.Freeze();

            return enlarged;
        }
    }
}
