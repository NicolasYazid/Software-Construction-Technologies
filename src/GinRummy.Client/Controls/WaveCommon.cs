using System;

namespace GinRummy.Client.Controls
{
    // Trigonometry used to build the paint field. The field needs around ten sine or cosine
    // values for every pixel and several hundred thousand pixels for every frame, so the
    // values are read from a table built once when the class loads instead of being computed
    // one by one. The error against the exact function is far below one level of colour, and
    // the frame costs a fraction of what it costs with the library functions. The lookup does
    // not call the library either: the floor and the folding into one turn are done with a
    // truncation and a mask, because on this framework each call to Math.Floor is a real call
    // and the field makes dozens of them for every pixel.
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
            // Truncation rounds towards zero, so a negative position steps one entry back to
            // reach its floor, and the mask then folds any number of whole turns into the
            // table, which lets an angle of any sign land inside it.
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
