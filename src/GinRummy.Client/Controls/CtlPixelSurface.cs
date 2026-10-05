using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    // Surface that draws the frame of a control as pixel art: the corners are rounded along a
    // quarter of a circle stepped in square blocks of the same module the painted background is
    // made of, so the button, the panel and the field read as part of the same grid as the
    // letters of the pixel typefaces. The shape is built from the size the layout grants, which
    // is why no bitmap is needed: nothing is stretched, the steps keep their size on a button
    // of any width, and a screen of any resolution draws them with the same sharp edge. No
    // border is drawn around the body, so a translucent colour keeps showing the background of
    // the screen the way a plain panel did. The depth, when it is asked for, is the same shape
    // painted underneath and pushed down a few blocks, which is what raises the element over
    // its own base. The content of the control is drawn over all of it untouched.
    public class CtlPixelSurface : Decorator
    {
        private const int DefaultCornerSteps = 4;
        private const int DefaultBaseDepth = 0;
        private const int BothSides = 2;
        private const int FirstBlock = 0;
        private const double Origin = 0.0;

        public static readonly DependencyProperty PixelUnitProperty =
            DependencyProperty.Register(
                "PixelUnit",
                typeof(double),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    PixelShapeCommon.DefaultPixelUnit,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        // The number of steps is also how many rows of the corner carry one, so a larger
        // element needs a larger number for its corner to read as round rather than square.
        public static readonly DependencyProperty CornerStepsProperty =
            DependencyProperty.Register(
                "CornerSteps",
                typeof(int),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    DefaultCornerSteps,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        // The face is the only colour that covers the shape, so a translucent one lets the
        // background of the screen through, which is what the panels of the menu are after.
        public static readonly DependencyProperty FaceBrushProperty =
            DependencyProperty.Register(
                "FaceBrush",
                typeof(Brush),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BaseBrushProperty =
            DependencyProperty.Register(
                "BaseBrush",
                typeof(Brush),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BaseDepthProperty =
            DependencyProperty.Register(
                "BaseDepth",
                typeof(int),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    DefaultBaseDepth,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        // A decorator has no padding of its own, and the content of a panel cannot sit on the
        // steps of the corners, so the surface declares one and honours it while it measures.
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                "Padding",
                typeof(Thickness),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double PixelUnit
        {
            get { return (double)GetValue(PixelUnitProperty); }
            set { SetValue(PixelUnitProperty, value); }
        }

        public int CornerSteps
        {
            get { return (int)GetValue(CornerStepsProperty); }
            set { SetValue(CornerStepsProperty, value); }
        }

        public Brush FaceBrush
        {
            get { return (Brush)GetValue(FaceBrushProperty); }
            set { SetValue(FaceBrushProperty, value); }
        }

        public Brush BaseBrush
        {
            get { return (Brush)GetValue(BaseBrushProperty); }
            set { SetValue(BaseBrushProperty, value); }
        }

        public int BaseDepth
        {
            get { return (int)GetValue(BaseDepthProperty); }
            set { SetValue(BaseDepthProperty, value); }
        }

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            PixelFrame frame = PixelShapeCommon.MeasureFrame(RenderSize, PixelUnit, CornerSteps);
            if (frame == null)
            {
                return;
            }

            DrawBase(drawingContext, frame);
            drawingContext.DrawGeometry(FaceBrush, null, PixelShapeCommon.BuildShape(frame));
        }

        // Redraws the shape whenever the layout grants the surface a different size, because
        // the shape is measured from that size and not from a fixed picture.
        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);
            InvalidateVisual();
        }

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
            if ((BaseBrush == null) || (depth <= FirstBlock))
            {
                return;
            }

            if (frame.Rows - depth < frame.Steps * BothSides)
            {
                return;
            }

            frame.Rows -= depth;
            Geometry body = PixelShapeCommon.BuildShape(frame);
            frame.OffsetY += depth * frame.Unit;
            Geometry seat = PixelShapeCommon.BuildShape(frame);
            frame.OffsetY -= depth * frame.Unit;

            drawingContext.DrawGeometry(
                BaseBrush,
                null,
                Geometry.Combine(seat, body, GeometryCombineMode.Exclude, null));
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
