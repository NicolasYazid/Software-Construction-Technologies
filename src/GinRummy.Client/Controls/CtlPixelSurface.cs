using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Surface that draws the frame of a control as pixel art: the corners are rounded along a
    /// quarter of a circle stepped in square blocks of the same module the painted background
    /// is made of, so the button, the panel and the field read as part of the same grid as the
    /// letters of the pixel typefaces.
    /// The shape is built from the size the layout grants, which is why no bitmap is needed:
    /// nothing is stretched, the steps keep their size on a button of any width, and a screen
    /// of any resolution draws them with the same sharp edge.
    /// No border is drawn around the body, so a translucent colour keeps showing the background
    /// of the screen the way a plain panel did. The depth, when it is asked for, is the same
    /// shape painted underneath and pushed down a few blocks, which is what raises the element
    /// over its own base. The content of the control is drawn over all of it untouched.
    /// </summary>
    public class CtlPixelSurface : Decorator
    {
        private const double DefaultPixelUnit = 3.0;
        private const int DefaultCornerSteps = 4;
        private const int DefaultBaseDepth = 0;
        private const int BothSides = 2;
        private const int FirstBlock = 0;
        private const int OneRow = 1;
        private const double Half = 0.5;
        private const double Origin = 0.0;

        /// <summary>
        /// Side of the block the shape is drawn on. Every measure of the surface is a whole
        /// number of these, so raising it makes the steps of the corners coarser.
        /// </summary>
        public static readonly DependencyProperty PixelUnitProperty =
            DependencyProperty.Register(
                "PixelUnit",
                typeof(double),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    DefaultPixelUnit,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Radius of the corner, counted in blocks. It is also how many rows of the corner
        /// carry a step, so a larger element needs a larger number for its corner to read as
        /// round rather than square.
        /// </summary>
        public static readonly DependencyProperty CornerStepsProperty =
            DependencyProperty.Register(
                "CornerSteps",
                typeof(int),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    DefaultCornerSteps,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Colour of the body of the surface. It is the only colour that covers the shape, so
        /// a translucent one lets the background of the screen through, which is what the
        /// panels of the menu are after.
        /// </summary>
        public static readonly DependencyProperty FaceBrushProperty =
            DependencyProperty.Register(
                "FaceBrush",
                typeof(Brush),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Colour of the base the body is raised over. Leaving it unset draws a flat surface.
        /// </summary>
        public static readonly DependencyProperty BaseBrushProperty =
            DependencyProperty.Register(
                "BaseBrush",
                typeof(Brush),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// How many blocks the body is raised over its base, which is how much of the base is
        /// seen along the bottom edge.
        /// </summary>
        public static readonly DependencyProperty BaseDepthProperty =
            DependencyProperty.Register(
                "BaseDepth",
                typeof(int),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    DefaultBaseDepth,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the colour of the base the body is raised over.
        /// </summary>
        public Brush BaseBrush
        {
            get { return (Brush)GetValue(BaseBrushProperty); }
            set { SetValue(BaseBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets how many blocks the body is raised over its base.
        /// </summary>
        public int BaseDepth
        {
            get { return (int)GetValue(BaseDepthProperty); }
            set { SetValue(BaseDepthProperty, value); }
        }

        /// <summary>
        /// Room left between the border of the surface and its content. A decorator has no
        /// padding of its own, and the content of a panel cannot sit on the steps of the
        /// corners, so the surface declares one and honours it while it measures.
        /// </summary>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                "Padding",
                typeof(Thickness),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the room left between the border of the surface and its content.
        /// </summary>
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the side of the block the shape is drawn on.
        /// </summary>
        public double PixelUnit
        {
            get { return (double)GetValue(PixelUnitProperty); }
            set { SetValue(PixelUnitProperty, value); }
        }

        /// <summary>
        /// Gets or sets how many blocks each corner gives up.
        /// </summary>
        public int CornerSteps
        {
            get { return (int)GetValue(CornerStepsProperty); }
            set { SetValue(CornerStepsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the colour of the body of the surface.
        /// </summary>
        public Brush FaceBrush
        {
            get { return (Brush)GetValue(FaceBrushProperty); }
            set { SetValue(FaceBrushProperty, value); }
        }

        /// <summary>
        /// Draws the surface underneath the content of the control.
        /// </summary>
        /// <param name="drawingContext">Where the shape is drawn.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            PixelFrame frame = MeasureFrame();
            if (frame == null)
            {
                return;
            }

            DrawBase(drawingContext, frame);
            drawingContext.DrawGeometry(FaceBrush, null, BuildShape(frame));
        }

        // The base is the same shape pushed down a few blocks and painted in the dark colour.
        // Both it and the body are shortened by that amount, so the element keeps the size the
        // layout gave it and the depth shows along its bottom edge instead of growing out of it.
        // Only the part of the base the body does not cover is painted: painting it whole would
        // leave an opaque layer behind the body, and a translucent colour would then be mixed
        // with that layer instead of with the screen. The frame is left ready for the body,
        // which is drawn right after.
        private void DrawBase(DrawingContext drawingContext, PixelFrame frame)
        {
            int depth = BaseDepth;
            if (BaseBrush == null || depth <= FirstBlock)
            {
                return;
            }

            if (frame.Rows - depth < frame.Steps * BothSides)
            {
                return;
            }

            frame.Rows -= depth;
            Geometry body = BuildShape(frame);
            frame.OffsetY += depth * frame.Unit;
            Geometry seat = BuildShape(frame);
            frame.OffsetY -= depth * frame.Unit;

            drawingContext.DrawGeometry(
                BaseBrush,
                null,
                Geometry.Combine(seat, body, GeometryCombineMode.Exclude, null));
        }

        /// <summary>
        /// Redraws the shape whenever the layout grants the surface a different size, because
        /// the shape is measured from that size and not from a fixed picture.
        /// </summary>
        /// <param name="info">Size the surface had and size it has now.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);
            InvalidateVisual();
        }

        // The grid is centred on the surface: the part of a block that does not fit whole is
        // shared between the two edges instead of piling up on one of them, so a control whose
        // size is not a multiple of the block is still framed evenly.
        private PixelFrame MeasureFrame()
        {
            PixelFrame frame = null;
            double unit = PixelUnit;
            int smallestSide = (BothSides * CornerSteps) + BothSides;
            int columns = 0;
            int rows = 0;

            if (unit > Origin)
            {
                columns = (int)(RenderSize.Width / unit);
                rows = (int)(RenderSize.Height / unit);
            }

            if (columns >= smallestSide && rows >= smallestSide)
            {
                frame = new PixelFrame
                {
                    Unit = unit,
                    Columns = columns,
                    Rows = rows,
                    Steps = CornerSteps,
                    Profile = BuildProfile(CornerSteps),
                    OffsetX = (RenderSize.Width - (columns * unit)) * Half,
                    OffsetY = (RenderSize.Height - (rows * unit)) * Half
                };
            }

            return frame;
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

        private static Geometry BuildShape(PixelFrame frame)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                TracePerimeter(context, frame);
            }

            geometry.Freeze();

            return geometry;
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

        /// <summary>
        /// Leaves room for the padding around the content and asks for the size the content
        /// needs plus that room.
        /// </summary>
        /// <param name="constraint">Size the parent offers.</param>
        /// <returns>The size this surface needs.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Thickness padding = ResolvePadding();
            Size needed = new Size(
                padding.Left + padding.Right,
                padding.Top + padding.Bottom);

            if (Child != null)
            {
                Child.Measure(Shrink(constraint, padding));
                needed.Width += Child.DesiredSize.Width;
                needed.Height += Child.DesiredSize.Height;
            }

            return needed;
        }

        // What the base takes along the bottom edge counts as padding: the content belongs to
        // the body, which is raised over it, so it cannot use that strip.
        private Thickness ResolvePadding()
        {
            Thickness padding = Padding;

            return new Thickness(
                padding.Left,
                padding.Top,
                padding.Right,
                padding.Bottom + (BaseDepth * PixelUnit));
        }

        /// <summary>
        /// Places the content inside the padding, which leaves the border and the steps of the
        /// corners clear of it.
        /// </summary>
        /// <param name="arrangeSize">Size the parent grants.</param>
        /// <returns>The size this surface takes.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child != null)
            {
                Thickness padding = ResolvePadding();
                Size inner = Shrink(arrangeSize, padding);
                Child.Arrange(new Rect(
                    padding.Left,
                    padding.Top,
                    inner.Width,
                    inner.Height));
            }

            return arrangeSize;
        }

        private static Size Shrink(Size size, Thickness padding)
        {
            double width = size.Width - padding.Left - padding.Right;
            double height = size.Height - padding.Top - padding.Bottom;

            return new Size(
                width > Origin ? width : Origin,
                height > Origin ? height : Origin);
        }
    }
}
