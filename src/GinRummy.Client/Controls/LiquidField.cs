namespace GinRummy.Client.Controls
{
    /// <summary>
    /// The two values that describe the liquid at one point: where the point sits between the
    /// deep colour and the body colour, and how much crest light it carries. Both are read
    /// from the same warped sample, so the light follows the flow of the liquid instead of
    /// lying over the screen as a fixed pattern.
    /// </summary>
    internal struct LiquidField
    {
        private readonly double _band;
        private readonly double _glow;

        /// <summary>
        /// Builds the description of one point of the liquid.
        /// </summary>
        /// <param name="band">Position between the deep colour and the body colour.</param>
        /// <param name="glow">Weight of the crest colour over the result.</param>
        internal LiquidField(double band, double glow)
        {
            _band = band;
            _glow = glow;
        }

        /// <summary>
        /// Gets the position between the deep colour and the body colour.
        /// </summary>
        internal double Band
        {
            get { return _band; }
        }

        /// <summary>
        /// Gets the weight of the crest colour over the result.
        /// </summary>
        internal double Glow
        {
            get { return _glow; }
        }
    }
}
