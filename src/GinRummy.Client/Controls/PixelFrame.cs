using System.Windows;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// The grid of blocks one pixel surface is drawn on. It holds the size of the block, how
    /// many of them fit on each axis and where the first one starts, so that every point of
    /// the shape is asked for in blocks and never in device units.
    /// </summary>
    internal sealed class PixelFrame
    {
        /// <summary>
        /// Gets or sets the side of one block.
        /// </summary>
        internal double Unit { get; set; }

        /// <summary>
        /// Gets or sets how many blocks fit across the surface.
        /// </summary>
        internal int Columns { get; set; }

        /// <summary>
        /// Gets or sets how many blocks fit down the surface.
        /// </summary>
        internal int Rows { get; set; }

        /// <summary>
        /// Gets or sets how many blocks each corner gives up to the steps, which is the radius
        /// of the corner measured in blocks.
        /// </summary>
        internal int Steps { get; set; }

        /// <summary>
        /// Gets or sets how far into the surface each row of a corner starts, counted in
        /// blocks from the edge. It holds one more entry than the corner has rows, and that
        /// last one is always zero, so the walk of the perimeter can ask for the row that
        /// follows the corner without checking whether it exists.
        /// </summary>
        internal int[] Profile { get; set; }

        /// <summary>
        /// Gets or sets the distance from the left edge of the surface to the first block.
        /// </summary>
        internal double OffsetX { get; set; }

        /// <summary>
        /// Gets or sets the distance from the top edge of the surface to the first block.
        /// </summary>
        internal double OffsetY { get; set; }

        /// <summary>
        /// Turns a corner of the grid of blocks into a point of the surface.
        /// </summary>
        /// <param name="column">Column of the grid, counted from the left.</param>
        /// <param name="row">Row of the grid, counted from the top.</param>
        /// <returns>The point that corner falls on.</returns>
        internal Point ToPoint(int column, int row)
        {
            return new Point(
                OffsetX + (column * Unit),
                OffsetY + (row * Unit));
        }
    }
}
