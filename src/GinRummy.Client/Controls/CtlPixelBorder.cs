using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    // Draws a border as pixel art with the same raised depth as the buttons.
    // A plain border clipped by PixelShapeCommon painted its bottom border inside the clip.
    // The dark strip then ate the last row of the lower corners, and the element showed fewer steps below than above.
    // This border keeps the face whole and draws the base underneath it instead.
    // The outline comes from the attached PixelShapeCommon.CornerSteps and PixelUnit properties.
    // The face comes from Background, the base from BorderBrush, and the depth from the bottom border thickness.
    // The depth is rounded to whole blocks, so every style written for a plain border keeps working unchanged.
    public class CtlPixelBorder : Border
    {
        private const int NoDepth = 0;
        private const int MinimumDepth = 1;
        private const int BothSides = 2;
        private const double NoThickness = 0.0;

        protected override void OnRender(DrawingContext drawingContext)
        {
            double unit = PixelShapeCommon.GetPixelUnit(this);
            PixelFrame frame = PixelShapeCommon.MeasureFrame(
                RenderSize,
                unit,
                PixelShapeCommon.GetCornerSteps(this));
            if (frame == null)
            {
                base.OnRender(drawingContext);
                return;
            }

            Geometry face = PixelShapeCommon.BuildShape(frame);
            int depth = ResolveDepth(unit);
            if (CanRaise(frame, depth))
            {
                Geometry outline = face;
                frame.Rows -= depth;
                face = PixelShapeCommon.BuildShape(frame);
                drawingContext.DrawGeometry(
                    BorderBrush,
                    null,
                    Geometry.Combine(outline, face, GeometryCombineMode.Exclude, null));
            }

            drawingContext.DrawGeometry(Background, null, face);
        }

        // The shape is measured from the size the layout grants and not from a fixed picture.
        // It must therefore be drawn again whenever that size changes.
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();
        }

        // The attached outline properties do not belong to the border.
        // They do not ask for a new render by themselves, so the shape is invalidated here when they change.
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            bool isOutlineProperty = (e.Property == PixelShapeCommon.CornerStepsProperty)
                || (e.Property == PixelShapeCommon.PixelUnitProperty);
            if (isOutlineProperty)
            {
                InvalidateVisual();
            }
        }

        // The bottom border of the styles was measured in device units, and not every one of them is a whole number of blocks.
        // Rounding keeps the base on the grid.
        // Any declared border keeps at least one block so that it never vanishes.
        private int ResolveDepth(double unit)
        {
            int depth = NoDepth;
            double bottom = BorderThickness.Bottom;
            if ((BorderBrush != null) && (bottom > NoThickness))
            {
                depth = Math.Max(MinimumDepth, (int)Math.Round(bottom / unit));
            }

            return depth;
        }

        // A face shorter than its two corners cannot carry them.
        // Such an element is drawn flat instead of with a broken outline.
        // The limit is the same one CtlPixelSurface uses.
        private static bool CanRaise(PixelFrame frame, int depth)
        {
            bool hasDepth = depth > NoDepth;
            bool hasRoomForCorners = (frame.Rows - depth) >= (frame.Steps * BothSides);

            return hasDepth && hasRoomForCorners;
        }
    }
}
