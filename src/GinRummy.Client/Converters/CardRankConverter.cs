using System;
using System.Globalization;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;

namespace GinRummy.Client.Converters
{
    // Writes the rank of a card as its corner shows it.
    // The cards from the two to the ten show their number, and the ace and the face cards show the letter of the dictionary.
    // The first value of the binding is the rank.
    // The second is the active culture, bound to the localization provider so that the letter is resolved again when the culture changes.
    public sealed class CardRankConverter : IMultiValueConverter
    {
        private const string AceKey = "Card_RankAce";
        private const string JackKey = "Card_RankJack";
        private const string QueenKey = "Card_RankQueen";
        private const string KingKey = "Card_RankKing";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            if ((values.Length > 0) && (values[0] is CardRank rank))
            {
                result = ToLabel(rank, LocalizationProvider.Instance);
            }

            return result;
        }

        // Not supported: the rank of a card is only shown, never written back.
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
