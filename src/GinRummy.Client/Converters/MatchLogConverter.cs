using System;
using System.Globalization;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;

namespace GinRummy.Client.Converters
{
    /// <summary>
    /// Writes an entry of the match log as the sentence of the dictionary for its event, with
    /// the card or the number the event carries. The first value of the binding is the entry;
    /// the second is the active culture, bound to the localization provider so that the
    /// sentence is written again when the culture changes.
    /// </summary>
    public sealed class MatchLogConverter : IMultiValueConverter
    {
        private const string DealtKey = "GameTable_LogDealtFormat";
        private const string TurnedUpKey = "GameTable_LogTurnedUpFormat";
        private const string PassedKey = "GameTable_LogPassed";
        private const string DrewFromStockKey = "GameTable_LogDrewFromStock";
        private const string DiscardedKey = "GameTable_LogDiscardedFormat";
        private const string KnockedKey = "GameTable_LogKnockedFormat";
        private const string GinKey = "GameTable_LogGin";
        private const string WaitingKey = "GameTable_LblWaiting";

        /// <summary>
        /// Returns the sentence of the entry in the active culture.
        /// </summary>
        /// <param name="values">The entry followed by the active culture.</param>
        /// <param name="targetType">Type requested by the binding.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Culture supplied by the binding, which is not the active
        /// one.</param>
        /// <returns>The sentence, or an empty string while there is no entry.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            MatchLogEntryDto entry = values.Length > 0 ? values[0] as MatchLogEntryDto : null;
            if (entry != null)
            {
                result = ToSentence(entry, LocalizationProvider.Instance);
            }

            return result;
        }

        /// <summary>
        /// Not supported: an entry of the log is only shown, never written back.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetTypes">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns: the operation is not supported.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("An entry of the match log is never written back.");
        }

        private static string ToSentence(MatchLogEntryDto entry, LocalizationProvider localization)
        {
            string sentence;
            switch (entry.Kind)
            {
                case MatchLogKind.Dealt:
                    sentence = localization.Format(DealtKey, entry.Amount);
                    break;
                case MatchLogKind.TurnedUp:
                    sentence = localization.Format(TurnedUpKey, CardNameConverter.ToName(entry.Card, localization));
                    break;
                case MatchLogKind.Passed:
                    sentence = localization.GetText(PassedKey);
                    break;
                case MatchLogKind.DrewFromStock:
                    sentence = localization.GetText(DrewFromStockKey);
                    break;
                case MatchLogKind.Discarded:
                    sentence = localization.Format(DiscardedKey, CardNameConverter.ToName(entry.Card, localization));
                    break;
                case MatchLogKind.Knocked:
                    sentence = localization.Format(KnockedKey, entry.Amount);
                    break;
                case MatchLogKind.Gin:
                    sentence = localization.GetText(GinKey);
                    break;
                default:
                    sentence = localization.GetText(WaitingKey);
                    break;
            }

            return sentence;
        }
    }
}
