using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Animated paint used by the background of the main menu and by the side panel of the
    /// other screens. The field turns around the centre and is folded on itself several times,
    /// which breaks the bands into strokes instead of the rings a plain swirl draws.
    /// It is deliberately computed at a very low resolution and stretched with nearest
    /// neighbour, so the result is made of visible square blocks. That is what ties it to the
    /// pixel typefaces of the client, and it also makes each frame cheap: the surface is a few
    /// thousand pixels, not a few hundred thousand.
    /// The three colours are properties, so the screen that hosts the control owns the palette
    /// and the control owns only the movement.
    /// </summary>
    public partial class CtlPaintBackground : UserControl
    {
        private const int BytesPerPixel = 4;
        private const int BlueOffset = 0;
        private const int GreenOffset = 1;
        private const int RedOffset = 2;
        private const int AlphaOffset = 3;
        private const int ByteMask = 0xFF;
        private const int GreenShift = 8;
        private const int RedShift = 16;
        private const int AlphaShift = 24;

        private const int DefaultShortSide = 150;
        private const double DefaultPatternScale = 1.0;
        private const int MinimumSideInPixels = 2;
        private const double BitmapDotsPerInch = 96.0;
        private const double DefaultAspectRatio = 1.0;

        private const int FoldCount = 5;
        private const double SpinEase = 0.5;
        private const double SpinAmount = 1.05;
        private const double Contrast = 1.0;
        private const double SpinOffset = 302.2;
        private const double SpinSpeed = 0.2;
        private const double SpinReach = 20.0;
        private const double FieldZoom = 30.0;
        private const double DriftSpeed = 2.0;
        private const double FoldPhase = 5.1123314;
        private const double FoldWeightY = 0.353;
        private const double FoldDriftA = 0.131121;
        private const double FoldDriftB = 0.113;
        private const double FoldSkew = 0.711;
        private const double FoldStep = 0.5;
        private const double ContrastWeight = 0.25;
        private const double SpinWeight = 0.5;
        private const double ContrastBias = 1.2;
        private const double PaintScale = 0.035;
        private const double PaintCeiling = 2.0;
        private const double PaintKnee = 1.0;
        private const double PaintOuterSlope = 0.15;
        private const double BaseShare = 0.3;

        private const double VignetteStart = 0.24;
        private const double VignetteEnd = 1.10;
        private const double VignetteStrength = 0.85;
        private const double SmoothstepScale = 3.0;
        private const double SmoothstepSlope = 2.0;
        private const double Half = 0.5;
        private const double Unit = 1.0;
        private const double Zero = 0.0;

        private static readonly Color FallbackDeepColour = Color.FromRgb(6, 24, 15);
        private static readonly Color FallbackMidColour = Color.FromRgb(30, 103, 70);
        private static readonly Color FallbackGlowColour = Color.FromRgb(106, 210, 154);

        /// <summary>
        /// How much of the field the surface covers. The paint is normalised by the diagonal of
        /// the surface, so a small panel and a whole window show the same piece of the spiral
        /// and the strokes therefore come out as many times larger as the window is larger.
        /// Raising this number reaches further out into the field, which makes the strokes
        /// smaller: a window with twice the diagonal of a panel needs about twice the scale for
        /// them to measure the same on screen.
        /// </summary>
        public static readonly DependencyProperty PatternScaleProperty =
            DependencyProperty.Register(
                "PatternScale",
                typeof(double),
                typeof(CtlPaintBackground),
                new PropertyMetadata(DefaultPatternScale));

        /// <summary>
        /// Pixels of the short side the paint is computed at. The surface is stretched to the
        /// size of the control with nearest neighbour, so this number decides how large the
        /// visible blocks come out: a panel and a whole screen need different values for the
        /// blocks to measure the same on both.
        /// </summary>
        public static readonly DependencyProperty BlockResolutionProperty =
            DependencyProperty.Register(
                "BlockResolution",
                typeof(int),
                typeof(CtlPaintBackground),
                new PropertyMetadata(DefaultShortSide, OnBlockResolutionChanged));

        /// <summary>
        /// Colour of the deepest part of the paint.
        /// </summary>
        public static readonly DependencyProperty DeepColourProperty =
            DependencyProperty.Register(
                "DeepColour",
                typeof(Color),
                typeof(CtlPaintBackground),
                new PropertyMetadata(FallbackDeepColour));

        /// <summary>
        /// Colour of the body of the paint.
        /// </summary>
        public static readonly DependencyProperty MidColourProperty =
            DependencyProperty.Register(
                "MidColour",
                typeof(Color),
                typeof(CtlPaintBackground),
                new PropertyMetadata(FallbackMidColour));

        /// <summary>
        /// Colour of the light of the paint.
        /// </summary>
        public static readonly DependencyProperty GlowColourProperty =
            DependencyProperty.Register(
                "GlowColour",
                typeof(Color),
                typeof(CtlPaintBackground),
                new PropertyMetadata(FallbackGlowColour));

        private readonly Stopwatch _clock;

        private WriteableBitmap _surface;
        private byte[] _frameBuffer;
        private int _pixelWidth;
        private int _pixelHeight;
        private volatile bool _isDrawing;
        private volatile bool _hasFinishedFrame;
        private bool _isSubscribedToRendering;

        /// <summary>
        /// Builds the control and leaves it stopped until it becomes visible.
        /// </summary>
        public CtlPaintBackground()
        {
            InitializeComponent();
            _clock = new Stopwatch();
            SizeChanged += OnControlSizeChanged;
            IsVisibleChanged += OnControlVisibilityChanged;
            Unloaded += OnControlUnloaded;
        }

        /// <summary>
        /// Gets or sets how much of the field the surface covers.
        /// </summary>
        public double PatternScale
        {
            get { return (double)GetValue(PatternScaleProperty); }
            set { SetValue(PatternScaleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the pixels of the short side the paint is computed at.
        /// </summary>
        public int BlockResolution
        {
            get { return (int)GetValue(BlockResolutionProperty); }
            set { SetValue(BlockResolutionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the colour of the deepest part of the paint.
        /// </summary>
        public Color DeepColour
        {
            get { return (Color)GetValue(DeepColourProperty); }
            set { SetValue(DeepColourProperty, value); }
        }

        /// <summary>
        /// Gets or sets the colour of the body of the paint.
        /// </summary>
        public Color MidColour
        {
            get { return (Color)GetValue(MidColourProperty); }
            set { SetValue(MidColourProperty, value); }
        }

        /// <summary>
        /// Gets or sets the colour of the light of the paint.
        /// </summary>
        public Color GlowColour
        {
            get { return (Color)GetValue(GlowColourProperty); }
            set { SetValue(GlowColourProperty, value); }
        }

        private static void OnBlockResolutionChanged(
            DependencyObject source,
            DependencyPropertyChangedEventArgs arguments)
        {
            CtlPaintBackground control = source as CtlPaintBackground;
            if (control != null)
            {
                control.CreateSurface(control.RenderSize);
            }
        }

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateSurface(e.NewSize);
        }

        private void OnControlVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            StopAnimation();
        }

        private void OnRendering(object sender, EventArgs e)
        {
            PresentFinishedFrame();
            RequestNextFrame();
        }

        private void StartAnimation()
        {
            _clock.Start();
            if (!_isSubscribedToRendering)
            {
                CompositionTarget.Rendering += OnRendering;
                _isSubscribedToRendering = true;
            }
        }

        private void StopAnimation()
        {
            if (_isSubscribedToRendering)
            {
                CompositionTarget.Rendering -= OnRendering;
                _isSubscribedToRendering = false;
            }

            _clock.Stop();
        }

        private void CreateSurface(Size availableSize)
        {
            double shortSide = Math.Min(availableSize.Width, availableSize.Height);
            if (shortSide < MinimumSideInPixels)
            {
                return;
            }

            int shortSideInPixels = Math.Max(MinimumSideInPixels, BlockResolution);
            double aspectRatio = ResolveAspectRatio(availableSize);
            int width = (int)Math.Round(shortSideInPixels * aspectRatio);
            int height = shortSideInPixels;
            if (availableSize.Width < availableSize.Height)
            {
                width = shortSideInPixels;
                height = (int)Math.Round(shortSideInPixels / aspectRatio);
            }

            _pixelWidth = Math.Max(MinimumSideInPixels, width);
            _pixelHeight = Math.Max(MinimumSideInPixels, height);
            _frameBuffer = new byte[_pixelWidth * _pixelHeight * BytesPerPixel];
            _hasFinishedFrame = false;
            _surface = new WriteableBitmap(
                _pixelWidth,
                _pixelHeight,
                BitmapDotsPerInch,
                BitmapDotsPerInch,
                PixelFormats.Pbgra32,
                null);

            imgSurface.Source = _surface;
        }

        private double ResolveAspectRatio(Size availableSize)
        {
            double aspectRatio = DefaultAspectRatio;
            if (availableSize.Height > Zero)
            {
                aspectRatio = availableSize.Width / availableSize.Height;
            }

            return aspectRatio;
        }

        private void PresentFinishedFrame()
        {
            if (_surface == null)
            {
                return;
            }

            if (!_hasFinishedFrame)
            {
                return;
            }

            _surface.WritePixels(
                new Int32Rect(0, 0, _pixelWidth, _pixelHeight),
                _frameBuffer,
                _pixelWidth * BytesPerPixel,
                0);

            _hasFinishedFrame = false;
        }

        private void RequestNextFrame()
        {
            if (_surface == null)
            {
                return;
            }

            if (_isDrawing)
            {
                return;
            }

            if (_hasFinishedFrame)
            {
                return;
            }

            PaintFrameRequest request = new PaintFrameRequest
            {
                Buffer = _frameBuffer,
                Width = _pixelWidth,
                Height = _pixelHeight,
                ElapsedSeconds = _clock.Elapsed.TotalSeconds,
                PatternScale = PatternScale,
                Palette = new PaintPalette(DeepColour, MidColour, GlowColour)
            };

            _isDrawing = true;
            Task.Run(() => DrawFrameInBackground(request));
        }

        private void DrawFrameInBackground(PaintFrameRequest request)
        {
            try
            {
                Parallel.For(0, request.Height, rowIndex => DrawRow(rowIndex, request));
                _hasFinishedFrame = true;
            }
            finally
            {
                _isDrawing = false;
            }
        }

        private static void DrawRow(int rowIndex, PaintFrameRequest request)
        {
            // The two axes are divided by the same number so that the field keeps its shape on
            // any proportion of window; dividing each by its own side would flatten the spiral
            // into an ellipse. The centre of the surface is the centre of the spiral.
            double diagonal = Math.Sqrt((request.Width * request.Width)
                + (request.Height * request.Height));
            double reach = diagonal / request.PatternScale;
            double centreX = request.Width * Half;
            double centreY = request.Height * Half;
            double unitY = (rowIndex - centreY) / reach;

            // The frame of shade is measured on the short side and not on the reach of the
            // field, so it falls on the same place of the surface however far the paint reaches
            // into the field and whichever movement is drawn underneath it.
            double shortSide = Math.Min(request.Width, request.Height);
            double shadeY = (rowIndex - centreY) / shortSide;
            int rowStart = rowIndex * request.Width * BytesPerPixel;

            for (int columnIndex = 0; columnIndex < request.Width; columnIndex++)
            {
                double unitX = (columnIndex - centreX) / reach;
                double shadeX = (columnIndex - centreX) / shortSide;
                double shade = ComputeShade(Math.Sqrt((shadeX * shadeX) + (shadeY * shadeY)));
                PaintMix mix = ComputeMix(unitX, unitY, request.ElapsedSeconds);
                int packed = request.Palette.Blend(mix, shade);
                WritePixel(request.Buffer, rowStart + (columnIndex * BytesPerPixel), packed);
            }
        }

        // The darkening starts away from the centre and eases in instead of growing with the
        // radius from the first pixel. A straight ramp tinted the middle of the screen, which is
        // where the wordmark and the bar of the menu sit, and left the corners too light.
        private static double ComputeShade(double radius)
        {
            double reach = (radius - VignetteStart) / (VignetteEnd - VignetteStart);
            double ramp = Clamp(reach, Zero, Unit);
            double eased = ramp * ramp * (SmoothstepScale - (SmoothstepSlope * ramp));

            return Unit - (VignetteStrength * eased);
        }

        // Past the knee the resolution of the paint advances far more slowly. Without this it
        // saturates well before the corners of a window as wide as the menu, and everything
        // beyond that radius comes out as the flat light colour instead of strokes.
        private static double Compress(double spread)
        {
            double head = Math.Min(spread, PaintKnee);
            double tail = Math.Max(spread - PaintKnee, Zero) * PaintOuterSlope;

            return Clamp(head + tail, Zero, PaintCeiling);
        }

        // The point is first turned around the centre by an angle that grows with its distance,
        // which is what curves the strokes, and the result is then folded on itself five times.
        // Each fold displaces the point by a wave that reads the point itself, so the field
        // never repeats and the seams of a plain swirl disappear.
        private static PaintMix ComputeMix(double unitX, double unitY, double elapsedSeconds)
        {
            double reach = Math.Sqrt((unitX * unitX) + (unitY * unitY));
            double turn = Math.Atan2(unitY, unitX)
                + (elapsedSeconds * SpinEase * SpinSpeed) + SpinOffset
                - (SpinEase * SpinReach * ((SpinAmount * reach) + (Unit - SpinAmount)));

            double fieldX = reach * WaveCommon.Cosine(turn) * FieldZoom;
            double fieldY = reach * WaveCommon.Sine(turn) * FieldZoom;
            double drift = elapsedSeconds * DriftSpeed;
            double foldX = fieldX + fieldY;
            double foldY = foldX;

            for (int foldIndex = 0; foldIndex < FoldCount; foldIndex++)
            {
                double largest = Math.Max(fieldX, fieldY);
                foldX += WaveCommon.Sine(largest) + fieldX;
                foldY += WaveCommon.Sine(largest) + fieldY;
                fieldX += FoldStep * WaveCommon.Cosine(
                    FoldPhase + (FoldWeightY * foldY) + (drift * FoldDriftA));
                fieldY += FoldStep * WaveCommon.Sine(foldX - (FoldDriftB * drift));
                double shear = WaveCommon.Cosine(fieldX + fieldY)
                    - WaveCommon.Sine((fieldX * FoldSkew) - fieldY);
                fieldX -= shear;
                fieldY -= shear;
            }

            double contrastMod = (ContrastWeight * Contrast) + (SpinWeight * SpinAmount) + ContrastBias;
            double spread = Math.Sqrt((fieldX * fieldX) + (fieldY * fieldY)) * PaintScale * contrastMod;
            double paint = Compress(spread);
            double deep = Math.Max(Zero, Unit - (contrastMod * Math.Abs(Unit - paint)));
            double mid = Math.Max(Zero, Unit - (contrastMod * Math.Abs(paint)));
            double glow = Unit - Math.Min(Unit, deep + mid);
            double baseShare = BaseShare / Contrast;

            return new PaintMix(
                baseShare + ((Unit - baseShare) * deep),
                (Unit - baseShare) * mid,
                (Unit - baseShare) * glow);
        }

        private static void WritePixel(byte[] buffer, int offset, int packedColour)
        {
            buffer[offset + BlueOffset] = (byte)(packedColour & ByteMask);
            buffer[offset + GreenOffset] = (byte)((packedColour >> GreenShift) & ByteMask);
            buffer[offset + RedOffset] = (byte)((packedColour >> RedShift) & ByteMask);
            buffer[offset + AlphaOffset] = (byte)((packedColour >> AlphaShift) & ByteMask);
        }

        private static double Clamp(double value, double lowest, double highest)
        {
            double clamped = value;
            if (clamped < lowest)
            {
                clamped = lowest;
            }
            else if (clamped > highest)
            {
                clamped = highest;
            }

            return clamped;
        }
    }
}
