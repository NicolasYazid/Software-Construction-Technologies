using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    // Draws the frame of a control as pixel art.
    // The corners are rounded along a quarter of a circle stepped in square blocks of the same module as the painted background.
    // The button, the panel and the field therefore read as part of the same grid as the letters of the pixel typefaces.
    // The shape is built from the size the layout grants, which is why no bitmap is needed.
    // Nothing is stretched, the steps keep their size on a button of any width, and any screen draws them with the same sharp edge.
    // No border is drawn around the body, so a translucent colour keeps showing the background of the screen like a plain panel.
    // When a depth is asked for, the same shape is painted underneath and pushed down a few blocks to raise the element over its base.
    // The content of the control is drawn over all of it untouched.
    public class CtlPixelSurface : Decorator
    {
        private const int DefaultCornerSteps = 4;
        private const int DefaultBaseDepth = 0;
        private const int BothSides = 2;
        private const int NoDepth = 0;
        private const double Origin = 0.0;

        public static readonly DependencyProperty PixelUnitProperty =
            DependencyProperty.Register(
                "PixelUnit",
                typeof(double),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    PixelShapeCommon.DefaultPixelUnit,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        // The number of steps is also how many rows of the corner carry one.
        // A larger element therefore needs a larger number for its corner to read as round rather than square.
        public static readonly DependencyProperty CornerStepsProperty =
            DependencyProperty.Register(
                "CornerSteps",
                typeof(int),
                typeof(CtlPixelSurface),
                new FrameworkPropertyMetadata(
                    DefaultCornerSteps,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        // The face is the only colour that covers the shape, so a translucent one lets the background of the screen through.
        // That is the effect the panels of the menu are after.
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

        // A decorator has no padding of its own, and the content of a panel cannot sit on the steps of the corners.
        // The surface therefore declares a padding and honours it while it measures.
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

        // The shape is measured from the size the layout grants and not from a fixed picture.
        // It must therefore be drawn again whenever that size changes.
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
        // Both the base and the body are shortened by that amount, so the element keeps the size the layout gave it.
        // The depth then shows along the bottom edge instead of growing out of it.
        // Only the part of the base the body does not cover is painted.
        // Painting it whole would leave an opaque layer behind the body, and a translucent colour would be mixed with it.
        // The frame is left ready for the body, which is drawn right after.
        private void DrawBase(DrawingContext drawingContext, PixelFrame frame)
        {
            int depth = BaseDepth;
            if ((BaseBrush == null) || (depth <= NoDepth))
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

        // What the base takes along the bottom edge counts as padding.
        // The content belongs to the body, which is raised over the base, so it cannot use that strip.
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
