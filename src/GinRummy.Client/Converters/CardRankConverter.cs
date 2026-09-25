using System;
using System.Globalization;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;

namespace GinRummy.Client.Converters
{
    /// <summary>
    /// Writes the rank of a card as its corner shows it: the number for the cards from the two
    /// to the ten, and the letter of the dictionary for the ace and the face cards. The first
    /// value of the binding is the rank; the second is the active culture, bound to the
    /// localization provider so that the letter is resolved again when the culture changes.
    /// </summary>
    public sealed class CardRankConverter : IMultiValueConverter
    {
        private const string AceKey = "Card_RankAce";
        private const string JackKey = "Card_RankJack";
        private const string QueenKey = "Card_RankQueen";
        private const string KingKey = "Card_RankKing";

        /// <summary>
        /// Returns the text of the rank in the active culture.
        /// </summary>
        /// <param name="values">The rank followed by the active culture.</param>
        /// <param name="targetType">Type requested by the binding.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Culture supplied by the binding, which is not the active
        /// one.</param>
        /// <returns>The text of the rank, or an empty string while there is no rank.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            if ((values.Length > 0) && (values[0] is CardRank rank))
            {
                result = ToLabel(rank, LocalizationProvider.Instance);
            }

            return result;
        }

        /// <summary>
        /// Not supported: the rank of a card is only shown, never written back.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetTypes">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns: the operation is not supported.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("The rank of a card is never written back.");
        }

        // Shared with the spoken name of a card, which starts with the same text.
        internal static string ToLabel(CardRank rank, LocalizationProvider localization)
        {
            string label;
            switch (rank)
            {
                case CardRank.Ace:
                    label = localization.GetText(AceKey);
                    break;
                case CardRank.Jack:
                    label = localization.GetText(JackKey);
                    break;
                case CardRank.Queen:
                    label = localization.GetText(QueenKey);
                    break;
                case CardRank.King:
                    label = localization.GetText(KingKey);
                    break;
                default:
                    label = ((int)rank).ToString(localization.CurrentCulture);
                    break;
            }

            return label;
        }
    }
}
