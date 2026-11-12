using System;
using System.Windows;

namespace GinRummy.Client.Controls
{
    // The angles and the return are translated from the script of "Balatro Card Tilt" by AloofMeepGamer.
    public class CardTilt
    {
        private const double SizeDivisor = 20.0;
        private const double ReleaseSeconds = 0.1;
        private const double FlatThresholdDegrees = 0.05;

        private bool _isFollowing;
        private double _releaseStartX;
        private double _releaseStartY;
        private double _releaseElapsed;

        public double AngleX { get; private set; }
        public double AngleY { get; private set; }

        public bool IsTilted
        {
            get { return (Math.Abs(AngleX) > FlatThresholdDegrees) || (Math.Abs(AngleY) > FlatThresholdDegrees); }
        }

        public void Follow(Vector pointerOffset, Size cardSize)
        {
            AngleX = pointerOffset.Y / (cardSize.Height / SizeDivisor);
            AngleY = -pointerOffset.X / (cardSize.Width / SizeDivisor);
            _isFollowing = true;
        }

        public void Release()
        {
            if (!_isFollowing)
            {
                return;
            }

            _isFollowing = false;
            _releaseStartX = AngleX;
            _releaseStartY = AngleY;
            _releaseElapsed = 0;
        }

        public void Step(double seconds)
        {
            if (_isFollowing)
            {
                return;
            }

            _releaseElapsed = Math.Min(_releaseElapsed + seconds, ReleaseSeconds);
            double progress = _releaseElapsed / ReleaseSeconds;
            double remaining = (1 - progress) * (1 - progress);
            AngleX = _releaseStartX * remaining;
            AngleY = _releaseStartY * remaining;
        }
    }
}
