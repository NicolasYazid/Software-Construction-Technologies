using System;

namespace GinRummy.Client.Controls
{
    /// <summary>
    /// Trigonometry used to build the liquid field. The field needs around ten sine or cosine
    /// values for every pixel and several hundred thousand pixels for every frame, so the
    /// values are read from a table built once when the class loads instead of being computed
    /// one by one. The error against the exact function is far below one level of colour, and
    /// the frame costs a fraction of what it costs with the library functions.
    /// </summary>
    internal static class WaveCommon
    {
        private const int TableSize = 4096;
        private const double TwoPi = Math.PI * 2.0;
        private const double QuarterTurn = Math.PI / 2.0;

        private static readonly double[] SineTable = BuildSineTable();

        /// <summary>
        /// Returns the sine of an angle expressed in radians.
        /// </summary>
        /// <param name="angle">Angle in radians, of any sign and any size.</param>
        /// <returns>The sine of the angle.</returns>
        internal static double Sine(double angle)
        {
            double turns = angle / TwoPi;
            turns -= Math.Floor(turns);

            double position = turns * TableSize;
            int lowIndex = (int)position;
            double fraction = position - lowIndex;
            double low = SineTable[lowIndex];
            double high = SineTable[lowIndex + 1];

            return low + ((high - low) * fraction);
        }

        /// <summary>
        /// Returns the cosine of an angle expressed in radians.
        /// </summary>
        /// <param name="angle">Angle in radians, of any sign and any size.</param>
        /// <returns>The cosine of the angle.</returns>
        internal static double Cosine(double angle)
        {
            return Sine(angle + QuarterTurn);
        }

        private static double[] BuildSineTable()
        {
            double[] table = new double[TableSize + 1];
            for (int index = 0; index <= TableSize; index++)
            {
                table[index] = Math.Sin(TwoPi * index / TableSize);
            }

            return table;
        }
    }
}
