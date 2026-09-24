using System;
using System.Windows;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Builds the stepped outline that every pixel element of the client shares, and lends it to
    /// any element as a clip through the CornerSteps attached property. That is how a field, a
    /// card or the box of a check takes the same corners of square blocks as the buttons
    /// without changing the element it is made of.
    /// </summary>
    public static class PixelShapeCommon
    {
        /// <summary>
        /// Side of the block the outline is drawn on when an element does not ask for another.
        /// It is the one the buttons and the panels of the menu use.
        /// </summary>
        public const double DefaultPixelUnit = 3.0;

        private const int NoSteps = 0;
        private const int BothSides = 2;
        private const int FirstBlock = 0;
        private const int OneRow = 1;
        private const double Half = 0.5;
        private const double Origin = 0.0;

        /// <summary>
        /// Radius of the corner of the outline that clips the element, counted in blocks. Zero,
        /// the default, leaves the element as it is.
        /// </summary>
        public static readonly DependencyProperty CornerStepsProperty =
            DependencyProperty.RegisterAttached(
                "CornerSteps",
                typeof(int),
                typeof(PixelShapeCommon),
                new PropertyMetadata(NoSteps, OnOutlineChanged));

        /// <summary>
        /// Side of the block of the outline that clips the element. Small elements take a
        /// smaller block so that their corners still read as round.
        /// </summary>
        public static readonly DependencyProperty PixelUnitProperty =
            DependencyProperty.RegisterAttached(
                "PixelUnit",
                typeof(double),
                typeof(PixelShapeCommon),
                new PropertyMetadata(DefaultPixelUnit, OnOutlineChanged));

        /// <summary>
        /// Gets the radius of the corner, in blocks, of the outline that clips the element.
        /// </summary>
        /// <param name="element">Element that carries the outline.</param>
        /// <returns>The radius of the corner in blocks.</returns>
        public static int GetCornerSteps(DependencyObject element)
        {
            return (int)element.GetValue(CornerStepsProperty);
        }

        /// <summary>
        /// Sets the radius of the corner, in blocks, of the outline that clips the element.
        /// </summary>
        /// <param name="element">Element that carries the outline.</param>
        /// <param name="value">Radius of the corner in blocks.</param>
        public static void SetCornerSteps(DependencyObject element, int value)
        {
            element.SetValue(CornerStepsProperty, value);
        }

        /// <summary>
        /// Gets the side of the block of the outline that clips the element.
        /// </summary>
        /// <param name="element">Element that carries the outline.</param>
        /// <returns>The side of the block.</returns>
        public static double GetPixelUnit(DependencyObject element)
        {
            return (double)element.GetValue(PixelUnitProperty);
        }

        /// <summary>
        /// Sets the side of the block of the outline that clips the element.
        /// </summary>
        /// <param name="element">Element that carries the outline.</param>
        /// <param name="value">Side of the block.</param>
        public static void SetPixelUnit(DependencyObject element, double value)
        {
            element.SetValue(PixelUnitProperty, value);
        }

        // The grid is centred on the element: the part of a block that does not fit whole is
        // shared between the two edges instead of piling up on one of them, so an element whose
        // size is not a multiple of the block is still framed evenly. An element too small for
        // its corner gets no frame at all.
        internal static PixelFrame MeasureFrame(Size size, double unit, int steps)
        {
            PixelFrame frame = null;
            int smallestSide = (BothSides * steps) + BothSides;
            int columns = 0;
            int rows = 0;

            if (unit > Origin)
            {
                columns = (int)(size.Width / unit);
                rows = (int)(size.Height / unit);
            }

            if (columns >= smallestSide && rows >= smallestSide)
            {
                frame = new PixelFrame
                {
                    Unit = unit,
                    Columns = columns,
                    Rows = rows,
                    Steps = steps,
                    Profile = BuildProfile(steps),
                    OffsetX = (size.Width - (columns * unit)) * Half,
                    OffsetY = (size.Height - (rows * unit)) * Half
                };
            }

            return frame;
        }

        internal static Geometry BuildShape(PixelFrame frame)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                TracePerimeter(context, frame);
            }

            geometry.Freeze();

            return geometry;
        }

        // The corner follows a quarter of a circle and not a straight diagonal. A diagonal
        // gives every row the same step, and a shape whose four corners are cut at the same
        // angle reads as an octagon; the circle gives the first row a wide step and the last
        // ones none at all, which is how a rounded corner is drawn on a grid of pixels.
        private static int[] BuildProfile(int steps)
        {
            int[] profile = new int[steps + OneRow];
            double radius = steps;

            for (int row = 0; row < steps; row++)
            {
                double height = radius - row - Half;
                double reach = Math.Sqrt((radius * radius) - (height * height));
                profile[row] = (int)((radius - reach) + Half);
            }

            return profile;
        }

        // The four corners are the same profile read in the four directions. Each row of a
        // corner contributes two moves, one across the row and one through it, so the edge
        // comes out as a staircase of uneven steps: wide where the circle is flat and narrow
        // where it turns. Walking the whole perimeter in blocks is what keeps every step
        // square and on the grid.
        private static void TracePerimeter(StreamGeometryContext context, PixelFrame frame)
        {
            int steps = frame.Steps;
            int[] profile = frame.Profile;
            int columns = frame.Columns;
            int rows = frame.Rows;

            context.BeginFigure(frame.ToPoint(profile[FirstBlock], FirstBlock), true, true);
            LineTo(context, frame.ToPoint(columns - profile[FirstBlock], FirstBlock));

            for (int row = 0; row < steps; row++)
            {
                LineTo(context, frame.ToPoint(columns - profile[row], row + OneRow));
                LineTo(context, frame.ToPoint(columns - profile[row + OneRow], row + OneRow));
            }

            LineTo(context, frame.ToPoint(columns, rows - steps));

            for (int row = steps - OneRow; row >= 0; row--)
            {
                LineTo(context, frame.ToPoint(columns - profile[row], rows - row - OneRow));
                LineTo(context, frame.ToPoint(columns - profile[row], rows - row));
            }

            LineTo(context, frame.ToPoint(profile[FirstBlock], rows));

            for (int row = 0; row < steps; row++)
            {
                LineTo(context, frame.ToPoint(profile[row], rows - row - OneRow));
                LineTo(context, frame.ToPoint(profile[row + OneRow], rows - row - OneRow));
            }

            LineTo(context, frame.ToPoint(FirstBlock, steps));

            for (int row = steps - OneRow; row >= 0; row--)
            {
                LineTo(context, frame.ToPoint(profile[row], row + OneRow));
                LineTo(context, frame.ToPoint(profile[row], row));
            }
        }

        private static void LineTo(StreamGeometryContext context, Point point)
        {
            context.LineTo(point, false, false);
        }

        // The outline is measured from the size the layout grants, so it follows the element
        // every time that size changes instead of being fixed once.
        private static void OnOutlineChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement target = element as FrameworkElement;
            if (target == null)
            {
                return;
            }

            target.SizeChanged -= OnTargetSizeChanged;
            if (GetCornerSteps(target) > NoSteps)
            {
                target.SizeChanged += OnTargetSizeChanged;
            }

            ApplyOutline(target);
        }

        private static void OnTargetSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyOutline((FrameworkElement)sender);
        }

        private static void ApplyOutline(FrameworkElement target)
        {
            Geometry outline = null;
            int steps = GetCornerSteps(target);

            if (steps > NoSteps)
            {
                Size size = new Size(target.ActualWidth, target.ActualHeight);
                PixelFrame frame = MeasureFrame(size, GetPixelUnit(target), steps);
                if (frame != null)
                {
                    outline = BuildShape(frame);
                }
            }

            target.Clip = outline;
        }
    }
}
