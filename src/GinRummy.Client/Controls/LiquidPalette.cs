using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// The three colours of the liquid background, read once per frame and mixed for every
    /// pixel. The result comes back already packed in the layout the bitmap expects, so the
    /// inner loop never touches a brush nor a dependency property.
    /// </summary>
    internal struct LiquidPalette
    {
        private const double NoIntensity = 0.0;
        private const double MaximumChannel = 255.0;
        private const int GreenShift = 8;
        private const int RedShift = 16;
        private const int AlphaShift = 24;
        private const int OpaqueAlpha = 255;

        private readonly Color _deep;
        private readonly Color _mid;
        private readonly Color _glow;

        /// <summary>
        /// Builds the palette from the three colours the hosting screen chose.
        /// </summary>
        /// <param name="deep">Colour of the deepest part of the liquid.</param>
        /// <param name="mid">Colour of the body of the liquid.</param>
        /// <param name="glow">Colour of the crests of the liquid.</param>
        internal LiquidPalette(Color deep, Color mid, Color glow)
        {
            _deep = deep;
            _mid = mid;
            _glow = glow;
        }

        /// <summary>
        /// Mixes the three colours for a single pixel.
        /// </summary>
        /// <param name="band">Position between the deep colour and the body colour.</param>
        /// <param name="glow">Weight of the crest colour over the result.</param>
        /// <param name="shade">Factor that darkens the pixel towards the edges.</param>
        /// <returns>The colour packed as alpha, red, green and blue.</returns>
        internal int Blend(double band, double glow, double shade)
        {
            double bodyRed = _deep.R + ((_mid.R - _deep.R) * band);
            double bodyGreen = _deep.G + ((_mid.G - _deep.G) * band);
            double bodyBlue = _deep.B + ((_mid.B - _deep.B) * band);

            int red = ToChannel((bodyRed + ((_glow.R - bodyRed) * glow)) * shade);
            int green = ToChannel((bodyGreen + ((_glow.G - bodyGreen) * glow)) * shade);
            int blue = ToChannel((bodyBlue + ((_glow.B - bodyBlue) * glow)) * shade);

            return (OpaqueAlpha << AlphaShift) | (red << RedShift) | (green << GreenShift) | blue;
        }

        private static int ToChannel(double value)
        {
            double clamped = value;
            if (clamped < NoIntensity)
            {
                clamped = NoIntensity;
            }
            else if (clamped > MaximumChannel)
            {
                clamped = MaximumChannel;
            }

            return (int)clamped;
        }
    }
}
