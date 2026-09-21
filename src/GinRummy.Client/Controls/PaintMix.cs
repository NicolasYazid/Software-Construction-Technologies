namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Weight of each of the three colours of the paint at one point. The three add up to one,
    /// so the palette can mix them without normalising anything.
    /// </summary>
    internal struct PaintMix
    {
        private readonly double _deepWeight;
        private readonly double _midWeight;
        private readonly double _glowWeight;

        /// <summary>
        /// Builds the mix of one point of the paint.
        /// </summary>
        /// <param name="deepWeight">Weight of the deepest colour.</param>
        /// <param name="midWeight">Weight of the colour of the body.</param>
        /// <param name="glowWeight">Weight of the colour of the light.</param>
        internal PaintMix(double deepWeight, double midWeight, double glowWeight)
        {
            _deepWeight = deepWeight;
            _midWeight = midWeight;
            _glowWeight = glowWeight;
        }

        /// <summary>
        /// Gets the weight of the deepest colour.
        /// </summary>
        internal double DeepWeight
        {
            get { return _deepWeight; }
        }

        /// <summary>
        /// Gets the weight of the colour of the body.
        /// </summary>
        internal double MidWeight
        {
            get { return _midWeight; }
        }

        /// <summary>
        /// Gets the weight of the colour of the light.
        /// </summary>
        internal double GlowWeight
        {
            get { return _glowWeight; }
        }
    }
}
