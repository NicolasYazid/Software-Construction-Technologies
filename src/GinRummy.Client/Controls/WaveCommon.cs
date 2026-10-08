using System;

namespace GinRummy.Client.Controls
{
    // The paint field needs dozens of sine and cosine values for every pixel of every frame.
    // The values are therefore read from a table built once when the class loads instead of calling the library functions.
    // Interpolating between entries keeps the error against the exact function far below one level of colour.
    internal static class WaveCommon
    {
        private const int TableSize = 4096;
        private const int TableMask = TableSize - 1;
        private const double TwoPi = Math.PI * 2.0;
        private const double QuarterTurn = Math.PI / 2.0;
        private const double StepsPerRadian = TableSize / TwoPi;

        private static readonly double[] SineTable = BuildSineTable();

        internal static double Sine(double angle)
        {
            // On this framework each call to Math.Floor is a real call, so the floor is taken with a truncation instead.
            // Truncation rounds towards zero, so a negative position steps one entry back to reach its floor.
            // The mask folds whole turns of either sign into the table, which only works while the size of the table is a power of two.
            double position = angle * StepsPerRadian;
            int wholeSteps = (int)position;
            if (position < wholeSteps)
            {
                wholeSteps--;
            }

            double fraction = position - wholeSteps;
            int lowIndex = wholeSteps & TableMask;
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
