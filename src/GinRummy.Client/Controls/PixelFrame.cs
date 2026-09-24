using System.Windows;

namespace GinRummy.Client.Controls
{
    // The grid of blocks one pixel surface is drawn on. It holds the size of the block, how
    // many of them fit on each axis and where the first one starts, so that every point of
    // the shape is asked for in blocks and never in device units.
    internal sealed class PixelFrame
    {
        internal double Unit { get; set; }
        internal int Columns { get; set; }
        internal int Rows { get; set; }
        // The radius of the corner measured in blocks, which is also how many rows of the
        // corner carry a step.
        internal int Steps { get; set; }
        // It holds one more entry than the corner has rows, and that last one is always zero,
        // so the walk of the perimeter can ask for the row that follows the corner without
        // checking whether it exists.
        internal int[] Profile { get; set; }
        // The part of a block that does not fit whole is shared between the two edges, so the
        // first block starts this far from the edge of the surface.
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
