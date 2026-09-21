using System.Windows.Media;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// The three colours of the paint and the mixing of them. It is a value type so that each
    /// frame carries its own copy and the drawing thread never reads a colour that the
    /// interface thread is changing at the same time.
    /// </summary>
    internal struct PaintPalette
    {
        private const double ByteRange = 255.0;
        private const int OpaqueAlpha = 255;
        private const int AlphaShift = 24;
        private const int RedShift = 16;
        private const int GreenShift = 8;
        private const double MinimumChannel = 0.0;
        private const double MaximumChannel = 1.0;

        private readonly double _deepRed;
        private readonly double _deepGreen;
        private readonly double _deepBlue;
        private readonly double _midRed;
        private readonly double _midGreen;
        private readonly double _midBlue;
        private readonly double _glowRed;
        private readonly double _glowGreen;
        private readonly double _glowBlue;

        /// <summary>
        /// Builds the palette from the three colours the screen chooses.
        /// </summary>
        /// <param name="deepColour">Colour of the deepest part of the paint.</param>
        /// <param name="midColour">Colour of the body of the paint.</param>
        /// <param name="glowColour">Colour of the light of the paint.</param>
        internal PaintPalette(Color deepColour, Color midColour, Color glowColour)
        {
            _deepRed = deepColour.R / ByteRange;
            _deepGreen = deepColour.G / ByteRange;
            _deepBlue = deepColour.B / ByteRange;
            _midRed = midColour.R / ByteRange;
            _midGreen = midColour.G / ByteRange;
            _midBlue = midColour.B / ByteRange;
            _glowRed = glowColour.R / ByteRange;
            _glowGreen = glowColour.G / ByteRange;
            _glowBlue = glowColour.B / ByteRange;
        }

        /// <summary>
        /// Mixes the three colours with the weights of one point, darkens the result by the
        /// shade of that point and returns it already packed as the pixel format of the surface
        /// expects it.
        /// </summary>
        /// <param name="mix">Weights of the three colours at that point.</param>
        /// <param name="shade">One for the centre of the surface and less towards the edge.</param>
        /// <returns>The colour of the pixel, packed as alpha, red, green and blue.</returns>
        internal int Blend(PaintMix mix, double shade)
        {
            double red = ((_deepRed * mix.DeepWeight)
                + (_midRed * mix.MidWeight)
                + (_glowRed * mix.GlowWeight)) * shade;
            double green = ((_deepGreen * mix.DeepWeight)
                + (_midGreen * mix.MidWeight)
                + (_glowGreen * mix.GlowWeight)) * shade;
            double blue = ((_deepBlue * mix.DeepWeight)
                + (_midBlue * mix.MidWeight)
                + (_glowBlue * mix.GlowWeight)) * shade;

            return (OpaqueAlpha << AlphaShift)
                | (ToChannel(red) << RedShift)
                | (ToChannel(green) << GreenShift)
                | ToChannel(blue);
        }

        private static int ToChannel(double value)
        {
            double clamped = value;
            if (clamped < MinimumChannel)
            {
                clamped = MinimumChannel;
            }
            else if (clamped > MaximumChannel)
            {
                clamped = MaximumChannel;
            }

            return (int)(clamped * ByteRange);
        }
    }
}
