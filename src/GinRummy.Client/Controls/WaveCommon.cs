using System;

namespace GinRummy.Client.Controls
{
    // Trigonometry used to build the paint field. The field needs around ten sine or cosine
    // values for every pixel and several hundred thousand pixels for every frame, so the
    // values are read from a table built once when the class loads instead of being computed
    // one by one. The error against the exact function is far below one level of colour, and
    // the frame costs a fraction of what it costs with the library functions.
    internal static class WaveCommon
    {
        private const int TableSize = 4096;
        private const double TwoPi = Math.PI * 2.0;
        private const double QuarterTurn = Math.PI / 2.0;

        private static readonly double[] SineTable = BuildSineTable();

        internal static double Sine(double angle)
        {
            // The angle is folded into a single turn first, so an angle of any sign and any
            // size lands inside the table.
            double turns = angle / TwoPi;
            turns -= Math.Floor(turns);

            double position = turns * TableSize;
            int lowIndex = (int)position;
            double fraction = position - lowIndex;
            double low = SineTable[lowIndex];
            double high = SineTable[lowIndex + 1];

            return low + ((high - low) * fraction);
        }

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
