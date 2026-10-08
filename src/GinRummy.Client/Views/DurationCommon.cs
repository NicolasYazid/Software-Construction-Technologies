using System;
using System.Globalization;

namespace GinRummy.Client.Views
{
    // The countdowns of the screens show their durations in clock form.
    // The hours are kept whole instead of rolling into days.
    // The prototype writes a suspension of two days as 47:59:59 and not as one day and a remainder.
    internal static class DurationCommon
    {
        private const string LongFormat = "{0:00}:{1:00}:{2:00}";
        private const string ShortFormat = "{0:00}:{1:00}";

        internal static string ToClock(TimeSpan duration, CultureInfo culture)
        {
            TimeSpan positive = duration;
            if (positive < TimeSpan.Zero)
            {
                positive = TimeSpan.Zero;
            }

            int hours = (int)positive.TotalHours;
            string clock = string.Format(culture, ShortFormat, positive.Minutes, positive.Seconds);
            if (hours > 0)
            {
                clock = string.Format(culture, LongFormat, hours, positive.Minutes, positive.Seconds);
            }

            return clock;
        }
    }
}
