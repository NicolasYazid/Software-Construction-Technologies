using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Paints once the backs of the playing cards: the spiral of the backgrounds, still and in
    /// the reds of the buttons. Each size of card has its own surface with as many blocks as
    /// fit in it at two pixels each, and the surface is enlarged here block by block, so the
    /// card shows it at its own size and nothing smooths the blocks.
    /// </summary>
    public static class CardBackCommon
    {
        private const int BlockSize = 2;
        private const int BytesPerPixel = 4;
        private const double DotsPerInch = 96.0;
        private const int SmallColumns = 24;
        private const int SmallRows = 37;
        private const int LargeColumns = 45;
        private const int LargeRows = 63;
        private const double StillMoment = 12.0;
        private const double StillPatternScale = 1.4;

        private static readonly Color DeepRed = Color.FromRgb(38, 8, 6);
        private static readonly Color MidRed = Color.FromRgb(139, 44, 31);
        private static readonly Color GlowRed = Color.FromRgb(224, 110, 88);

        private static readonly ImageSource SmallSurface = PaintSurface(SmallColumns, SmallRows);
        private static readonly ImageSource LargeSurface = PaintSurface(LargeColumns, LargeRows);

        /// <summary>
        /// Gets the back of the small cards, the ones of the hand of the opponent.
        /// </summary>
        public static ImageSource Surface
        {
            get { return SmallSurface; }
        }

        /// <summary>
        /// Gets the back of the large cards, the one of the stock.
        /// </summary>
        public static ImageSource Large
        {
            get { return LargeSurface; }
        }

        private static ImageSource PaintSurface(int columns, int rows)
        {
            PaintFrameRequest request = new PaintFrameRequest
            {
                Buffer = new byte[columns * rows * BytesPerPixel],
                Width = columns,
                Height = rows,
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
