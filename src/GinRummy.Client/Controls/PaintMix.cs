namespace GinRummy.Client.Controls
{
    // Weight of each of the three colours of the paint at one point. The three add up to one,
    // so the palette can mix them without normalising anything.
    internal struct PaintMix
    {
        private readonly double _deepWeight;
        private readonly double _midWeight;
        private readonly double _glowWeight;

        internal PaintMix(double deepWeight, double midWeight, double glowWeight)
        {
            _deepWeight = deepWeight;
            _midWeight = midWeight;
            _glowWeight = glowWeight;
        }

        internal double DeepWeight
        {
            get { return _deepWeight; }
        }

        internal double MidWeight
        {
            get { return _midWeight; }
        }

        internal double GlowWeight
        {
            get { return _glowWeight; }
        }
    }
}
