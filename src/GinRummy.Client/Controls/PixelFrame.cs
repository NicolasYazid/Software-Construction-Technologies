using System.Windows;

namespace GinRummy.Client.Controls
{
    internal sealed class PixelFrame
    {
        internal double Unit { get; set; }
        internal int Columns { get; set; }
        internal int Rows { get; set; }
        internal int Steps { get; set; }
        // The profile holds one more entry than the corner has rows, and that last entry must stay zero.
        // The walk of the perimeter then reads the row that follows the corner without checking whether it exists.
        internal int[] Profile { get; set; }
        internal double OffsetX { get; set; }
        internal double OffsetY { get; set; }

        internal Point ToPoint(int column, int row)
        {
            return new Point(
                OffsetX + (column * Unit),
                OffsetY + (row * Unit));
        }
    }
}
