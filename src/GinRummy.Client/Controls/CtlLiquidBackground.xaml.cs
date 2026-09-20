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
    /// Animated liquid background of the client, written from scratch as CON-06 requires.
    /// Each frame is computed on a background thread into a small bitmap that the control
    /// stretches to its own size, so the effect needs no graphics pipeline, no shader profile
    /// and no external package, and it renders the same on every machine the game is reviewed
    /// on. The interface thread only copies the finished frame, so a slow frame lowers the
    /// frame rate of the background but never freezes the menu.
    /// The colour of the three bands is exposed as properties, so the palette belongs to the
    /// screen that hosts the control and not to the control itself.
    /// </summary>
    public partial class CtlLiquidBackground : UserControl
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

        private const int MaximumShortSide = 400;
        private const int MinimumSideInPixels = 2;
        private const double BitmapDotsPerInch = 96.0;
        private const double DefaultAspectRatio = 1.0;

        private const int FoldCount = 3;
        private const double SwirlStrength = 0.80;
        private const double SwirlSoftness = 0.18;
        private const double SpinSpeed = 0.10;
        private const double FieldZoom = 9.5;
        private const double BaseFrequency = 1.25;
        private const double WarpAmplitude = 1.05;
        private const double Lacunarity = 1.71;
        private const double Gain = 0.74;
        private const double DriftX = 0.43;
        private const double DriftY = 0.33;
        private const double FoldPhaseStep = 1.7;
        private const double BandContrast = 2.15;
        private const double BandBias = 0.14;
        private const double CrestFrequencyX = 0.70;
        private const double CrestFrequencyY = 1.10;
        private const double CrestSpeed = 0.26;
        private const double CrestThreshold = 0.60;
        private const double CrestAmount = 0.85;
        private const double VignetteAmount = 0.42;
        private const double CentreOffset = 0.5;
        private const double HalfRange = 0.5;
        private const double FullRange = 1.0;
        private const double NoIntensity = 0.0;

        private static readonly Color FallbackDeepColour = Color.FromRgb(62, 143, 104);
        private static readonly Color FallbackMidColour = Color.FromRgb(143, 203, 168);
        private static readonly Color FallbackGlowColour = Color.FromRgb(228, 244, 233);

        /// <summary>
        /// Colour of the deepest part of the liquid.
        /// </summary>
        public static readonly DependencyProperty DeepColourProperty =
            DependencyProperty.Register(
                "DeepColour",
                typeof(Color),
                typeof(CtlLiquidBackground),
                new PropertyMetadata(FallbackDeepColour));

        /// <summary>
        /// Colour of the body of the liquid.
        /// </summary>
        public static readonly DependencyProperty MidColourProperty =
            DependencyProperty.Register(
                "MidColour",
                typeof(Color),
                typeof(CtlLiquidBackground),
                new PropertyMetadata(FallbackMidColour));

        /// <summary>
        /// Colour of the crests, where the light of the scene gathers.
        /// </summary>
        public static readonly DependencyProperty GlowColourProperty =
            DependencyProperty.Register(
                "GlowColour",
                typeof(Color),
                typeof(CtlLiquidBackground),
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
        public CtlLiquidBackground()
        {
            InitializeComponent();
            _clock = new Stopwatch();
            SizeChanged += OnControlSizeChanged;
            IsVisibleChanged += OnControlVisibilityChanged;
            Unloaded += OnControlUnloaded;
        }

        /// <summary>
        /// Gets or sets the colour of the deepest part of the liquid.
        /// </summary>
        public Color DeepColour
        {
            get { return (Color)GetValue(DeepColourProperty); }
            set { SetValue(DeepColourProperty, value); }
        }

        /// <summary>
        /// Gets or sets the colour of the body of the liquid.
        /// </summary>
        public Color MidColour
        {
            get { return (Color)GetValue(MidColourProperty); }
            set { SetValue(MidColourProperty, value); }
        }

        /// <summary>
        /// Gets or sets the colour of the crests of the liquid.
        /// </summary>
        public Color GlowColour
        {
            get { return (Color)GetValue(GlowColourProperty); }
            set { SetValue(GlowColourProperty, value); }
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

            double aspectRatio = ResolveAspectRatio(availableSize);
            int width = (int)Math.Round(MaximumShortSide * aspectRatio);
            int height = MaximumShortSide;
            if (availableSize.Width < availableSize.Height)
            {
                width = MaximumShortSide;
                height = (int)Math.Round(MaximumShortSide / aspectRatio);
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
            if (availableSize.Height > NoIntensity)
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

            LiquidFrameRequest request = new LiquidFrameRequest
            {
                Buffer = _frameBuffer,
                Width = _pixelWidth,
                Height = _pixelHeight,
                ElapsedSeconds = _clock.Elapsed.TotalSeconds,
                Palette = new LiquidPalette(DeepColour, MidColour, GlowColour)
            };

            _isDrawing = true;
            Task.Run(() => DrawFrameInBackground(request));
        }

        private void DrawFrameInBackground(LiquidFrameRequest request)
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

        private static void DrawRow(int rowIndex, LiquidFrameRequest request)
        {
            double shortSide = Math.Min(request.Width, request.Height);
            double centreX = request.Width * CentreOffset;
            double centreY = request.Height * CentreOffset;
            double unitY = (rowIndex - centreY) / shortSide;
            int rowStart = rowIndex * request.Width * BytesPerPixel;

            for (int columnIndex = 0; columnIndex < request.Width; columnIndex++)
            {
                double unitX = (columnIndex - centreX) / shortSide;
                double radius = Math.Sqrt((unitX * unitX) + (unitY * unitY));
                LiquidField field = ComputeField(unitX, unitY, request.ElapsedSeconds);
                double shade = FullRange - (VignetteAmount * radius);
                int packed = request.Palette.Blend(field.Band, field.Glow, shade);
                WritePixel(request.Buffer, rowStart + (columnIndex * BytesPerPixel), packed);
            }
        }

        private static LiquidField ComputeField(double unitX, double unitY, double elapsedSeconds)
        {
            double radius = Math.Sqrt((unitX * unitX) + (unitY * unitY));
            double swirl = (SwirlStrength / (radius + SwirlSoftness)) + (elapsedSeconds * SpinSpeed);
            double swirlSine = WaveCommon.Sine(swirl);
            double swirlCosine = WaveCommon.Cosine(swirl);

            double sampleX = ((unitX * swirlCosine) - (unitY * swirlSine)) * FieldZoom;
            double sampleY = ((unitX * swirlSine) + (unitY * swirlCosine)) * FieldZoom;
            double frequency = BaseFrequency;
            double amplitude = WarpAmplitude;

            for (int foldIndex = 0; foldIndex < FoldCount; foldIndex++)
            {
                double phase = foldIndex * FoldPhaseStep;
                double previousX = sampleX;
                sampleX += amplitude * WaveCommon.Sine((sampleY * frequency) + (elapsedSeconds * DriftX) + phase);
                sampleY += amplitude * WaveCommon.Cosine((previousX * frequency) + (elapsedSeconds * DriftY) - phase);
                frequency *= Lacunarity;
                amplitude *= Gain;
            }

            double rawBand = HalfRange + (HalfRange * WaveCommon.Sine(sampleX + sampleY));
            double band = Clamp(HalfRange + ((rawBand - HalfRange) * BandContrast) - BandBias);

            // The crest rides the warped sample and not the screen coordinates. Reading it
            // from the screen drew straight diagonal bands over the whole background.
            double crestAngle = (sampleX * CrestFrequencyX)
                - (sampleY * CrestFrequencyY)
                + (elapsedSeconds * CrestSpeed);
            double rawCrest = HalfRange + (HalfRange * WaveCommon.Sine(crestAngle));
            double glow = Clamp((rawCrest - CrestThreshold) / (FullRange - CrestThreshold)) * CrestAmount;

            return new LiquidField(band, glow);
        }

        private static void WritePixel(byte[] buffer, int offset, int packedColour)
        {
            buffer[offset + BlueOffset] = (byte)(packedColour & ByteMask);
            buffer[offset + GreenOffset] = (byte)((packedColour >> GreenShift) & ByteMask);
            buffer[offset + RedOffset] = (byte)((packedColour >> RedShift) & ByteMask);
            buffer[offset + AlphaOffset] = (byte)((packedColour >> AlphaShift) & ByteMask);
        }

        private static double Clamp(double value)
        {
            double clamped = value;
            if (clamped < NoIntensity)
            {
                clamped = NoIntensity;
            }
            else if (clamped > FullRange)
            {
                clamped = FullRange;
            }

            return clamped;
        }
    }
}
