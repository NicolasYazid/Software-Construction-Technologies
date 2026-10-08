using System;
using System.Globalization;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;

namespace GinRummy.Client.Converters
{
    // The binding also passes the active culture, which Convert ignores, only so that the letter is resolved when the culture changes.
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("The rank of a card is never written back.");
        }

        // It is internal and static because the spoken name of a card reuses this label.
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
