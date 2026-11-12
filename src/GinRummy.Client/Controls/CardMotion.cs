using System;
using System.Windows;

namespace GinRummy.Client.Controls
{
    // The constants and the order of every step are translated from the card script of "Balatro Card Hover" by Alexx.
    public class CardMotion
    {
        public const double StepSeconds = 1.0 / 60.0;

        private const double FollowFactor = 0.25;
        private const double RotationDamping = 0.8;
        private const double LiftedScale = 1.05;
        private const double RestingScale = 1.0;
        private const double MaxDragSwing = 0.3;
        private const double MaxLeftReturnSwing = -0.3;
        private const double MaxRightReturnSwing = 0.25;
        private const double ReturnSwingFactor = 0.01532;
        private const double DragVelocityDivisor = 4000.0;
        private const double WobbleRotationAmplitude = 0.003625 / 2.0;
        private const double WobbleOffsetAmplitude = 0.875 / 2.0;
        private const double WobbleRotationPhase = 1321.0;
        private const double WobbleHorizontalPhase = 180.0 + 1321.0;
        private const double WobbleVerticalPhase = 360.0 + 1231.0;
        private const double DegreesPerRadian = 180.0 / Math.PI;
        private const double FlipSeconds = 0.13;

        private const double EntryFollowFactor = 0.271;

        private const double EntryArrivalDistance = 3.0;
        private const double ArrivalScaleTolerance = 0.02;
        private const double MinimumFlipWidth = 0.02;
        private const double Half = 0.5;

        private Point _previousPosition;
        private double _dragSwing;
        private double _time;
        private bool _isEntering;
        private double _exitScale;

        public CardMotion(Point startPosition, double phase)
        {
            Position = startPosition;
            _previousPosition = startPosition;
            _time = phase;
            Scale = RestingScale;
            Tilt = new CardTilt();
        }

        public Point Position { get; private set; }
        public Point Target { get; set; }
        public double SlotAngle { get; private set; }
        public double TargetSlotAngle { get; set; }
        public double Rotation { get; private set; }
        public double Scale { get; private set; }
        public bool IsDragged { get; set; }
        public bool IsHovered { get; set; }
        public CardTilt Tilt { get; private set; }

        public double FaceDownAmount { get; private set; }
        public bool IsLeaving { get; private set; }

        public bool HasArrived
        {
            get
            {
                bool isInPlace = (Target - Position).Length <= EntryArrivalDistance;
                bool isAtSize = !IsLeaving || (Math.Abs(Scale - _exitScale) <= ArrivalScaleTolerance);

                return isInPlace && isAtSize;
            }
        }

        public double AngleDegrees
        {
            get { return SlotAngle + (Rotation * DegreesPerRadian); }
        }

        public bool IsShowingBack
        {
            get { return FaceDownAmount > Half; }
        }

        public double FlipWidth
        {
            get { return Math.Max(MinimumFlipWidth, Math.Abs(Math.Cos(FaceDownAmount * Math.PI))); }
        }

        public void PrepareEntry(double startScale, bool isFaceDown)
        {
            Scale = startScale;
            FaceDownAmount = isFaceDown ? 1 : 0;
            _isEntering = true;
        }

        public void PrepareExit(double exitScale)
        {
            _exitScale = exitScale;
            _isEntering = false;
            IsLeaving = true;
        }

        public void RegisterDragVelocity(double horizontalVelocity)
        {
            _dragSwing = Clamp(horizontalVelocity / DragVelocityDivisor, -MaxDragSwing, MaxDragSwing);
        }

        public void Step()
        {
            if (IsDragged)
            {
                StepDragged();
            }
            else
            {
                StepResting();
            }

            SlotAngle = Lerp(SlotAngle, TargetSlotAngle, FollowFactor);
            FaceDownAmount = Math.Max(0, FaceDownAmount - (StepSeconds / FlipSeconds));
            Tilt.Step(StepSeconds);
            _time += StepSeconds;
        }

        private static double Lerp(double from, double to, double factor)
        {
            return from + ((to - from) * factor);
        }

        private static Point Lerp(Point from, Point to, double factor)
        {
            return new Point(Lerp(from.X, to.X, factor), Lerp(from.Y, to.Y, factor));
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        private void StepDragged()
        {
            Position = Lerp(Position, Target, FollowFactor);
            Rotation += _dragSwing;
            Rotation *= RotationDamping;
            Scale = Lerp(Scale, LiftedScale, FollowFactor);
            _dragSwing = 0;
        }

        private void StepResting()
        {
            bool isTravelling = _isEntering || IsLeaving;
            double followFactor = isTravelling ? EntryFollowFactor : FollowFactor;
            Position = Lerp(Position, Target, followFactor);
            _isEntering = _isEntering && ((Target - Position).Length > EntryArrivalDistance);
            double returnSwing = (Position.X - _previousPosition.X) * ReturnSwingFactor;
            _previousPosition = Position;
            Rotation += Clamp(returnSwing, MaxLeftReturnSwing, MaxRightReturnSwing);
            Rotation *= RotationDamping;
            Rotation += Math.Sin(_time + WobbleRotationPhase) * WobbleRotationAmplitude;
            Position = new Point(
                Position.X + (Math.Cos(_time + WobbleHorizontalPhase) * WobbleOffsetAmplitude),
                Position.Y + (Math.Sin(_time + WobbleVerticalPhase) * WobbleOffsetAmplitude));
            double targetScale = IsHovered ? LiftedScale : RestingScale;
            if (IsLeaving)
            {
                targetScale = _exitScale;
            }

            Scale = Lerp(Scale, targetScale, followFactor);
        }
    }
}
