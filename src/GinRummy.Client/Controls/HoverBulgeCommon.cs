using System;
using System.Windows;
using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    // The formula is translated from the vertex function of "Balatro Card Hover" by Alexx.
    // Godot moves each corner of the card on its own, while WPF can only transform the card as a whole.
    // The four corner movements are therefore replaced by the affine transform that fits them best.
    public static class HoverBulgeCommon
    {
        private const double PointerScale = 2.0;
        private const double MaxPointerOffset = 2000.0;
        private const double BaseFactor = 0.2;
        private const double EdgeBias = -0.03;
        private const double CenterWeight = 0.3;
        private const double CenterRadius = 0.3;
        private const double DistanceOffset = 2.0;
        private const double ScreenScale = 1.0;
        private const double Half = 0.5;
        private const double Quarter = 0.25;

        private static readonly double _unitDiagonal = Math.Sqrt(2.0);

        public static Matrix CreateMatrix(Size cardSize, Vector pointerOffset)
        {
            Vector pointer = new Vector(
                ClampOffset(pointerOffset.X * PointerScale),
                ClampOffset(pointerOffset.Y * PointerScale));
            // The original card is a centered Sprite2D, so its corners are measured from the middle of the card.
            double halfWidth = cardSize.Width * Half;
            double halfHeight = cardSize.Height * Half;
            double topLeft = GetDisplacement(new Vector(-halfWidth, -halfHeight), pointer);
            double topRight = GetDisplacement(new Vector(halfWidth, -halfHeight), pointer);
            double bottomLeft = GetDisplacement(new Vector(-halfWidth, halfHeight), pointer);
            double bottomRight = GetDisplacement(new Vector(halfWidth, halfHeight), pointer);
            double horizontalChange = ((topRight + bottomRight) - (topLeft + bottomLeft)) * Half;
            double verticalChange = ((bottomLeft + bottomRight) - (topLeft + topRight)) * Half;
            double offset = ((topLeft + topRight + bottomLeft + bottomRight) * Quarter) - ((horizontalChange + verticalChange) * Half);
            double horizontalRate = horizontalChange / cardSize.Width;
            double verticalRate = verticalChange / cardSize.Height;

            return new Matrix(1 + horizontalRate, horizontalRate, verticalRate, 1 + verticalRate, offset, offset);
        }

        private static double ClampOffset(double value)
        {
            return Math.Max(-MaxPointerOffset, Math.Min(MaxPointerOffset, value));
        }

        // Godot adds the same amount to both coordinates of the corner, so the corner moves along the diagonal.
        private static double GetDisplacement(Vector corner, Vector pointer)
        {
            double middleDistance = (corner - new Vector(Half, Half)).Length / _unitDiagonal;
            double squaredOffset = ((corner + pointer) / ScreenScale).LengthSquared;
            double weight = EdgeBias - (CenterWeight * Math.Max(0, CenterRadius - middleDistance));

            return BaseFactor * weight * squaredOffset / (DistanceOffset - middleDistance);
        }
    }
}
