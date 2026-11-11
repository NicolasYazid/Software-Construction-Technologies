using System;
using System.Windows;

namespace GinRummy.Client.Controls
{
    // The hand opens as a fan: the cards at the ends sit lower and lean outward, and the hand closes up when it no longer fits.
    public class HandFan
    {
        private const double CardGap = 2.0;
        private const double ArcDepth = 8.0;
        private const double EdgeAngleDegrees = 5.0;
        private const double Half = 0.5;
        private const double NormalBend = 1.0;
        private const double FlippedBend = -1.0;

        private readonly int _count;
        private readonly double _spacing;
        private readonly double _firstCenterX;
        private readonly double _centerY;

        public HandFan(int count, Size cardSize, Size area)
        {
            _count = count;
            double preferredSpacing = cardSize.Width + CardGap;
            double fittingSpacing = preferredSpacing;
            if (count > 1)
            {
                fittingSpacing = (area.Width - cardSize.Width) / (count - 1);
            }

            _spacing = Math.Max(0, Math.Min(preferredSpacing, fittingSpacing));
            _firstCenterX = (area.Width - (_spacing * (count - 1))) * Half;
            _centerY = area.Height * Half;
        }

        // The hand of the opponent is seen from the other side of the table, so its fan bends the other way.
        public bool IsFlipped { get; set; }

        public Point GetSlotCenter(int index)
        {
            double spread = GetSpread(index);

            return new Point(_firstCenterX + (index * _spacing), _centerY + (GetBend() * ArcDepth * spread * spread));
        }

        public double GetSlotAngle(int index)
        {
            return GetBend() * EdgeAngleDegrees * GetSpread(index);
        }

        public int GetNearestIndex(double horizontalPosition)
        {
            int nearestIndex = 0;
            if (_spacing > 0)
            {
                int roundedIndex = (int)Math.Round((horizontalPosition - _firstCenterX) / _spacing);
                nearestIndex = Math.Max(0, Math.Min(_count - 1, roundedIndex));
            }

            return nearestIndex;
        }

        private double GetBend()
        {
            return IsFlipped ? FlippedBend : NormalBend;
        }

        // The spread runs from -1 at the leftmost card to 1 at the rightmost one.
        private double GetSpread(int index)
        {
            double spread = 0;
            if (_count > 1)
            {
                spread = ((2.0 * index) / (_count - 1)) - 1;
            }

            return spread;
        }
    }
}
