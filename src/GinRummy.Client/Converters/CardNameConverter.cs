using System;
using System.Globalization;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;

namespace GinRummy.Client.Converters
{
    // The binding also passes the active culture, which Convert ignores, only so that the name is rebuilt when the culture changes.
    public sealed class CardNameConverter : IMultiValueConverter
    {
        private const string NameFormatKey = "Card_A11yFormat";
        private const string ClubsKey = "Card_SuitClubs";
        private const string DiamondsKey = "Card_SuitDiamonds";
        private const string HeartsKey = "Card_SuitHearts";
        private const string SpadesKey = "Card_SuitSpades";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            CardDto card = values.Length > 0 ? values[0] as CardDto : null;
            if (card != null)
            {
                result = ToName(card, LocalizationProvider.Instance);
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("The name of a card is never written back.");
        }

        // It is internal and static because the match log names its cards with this same text.
        // The order of the rank and the suit comes from the format, since it changes between languages.
        internal static string ToName(CardDto card, LocalizationProvider localization)
        {
            return localization.Format(
                NameFormatKey,
                CardRankConverter.ToLabel(card.Rank, localization),
                localization.GetText(GetSuitKey(card.Suit)));
        }

        private static string GetSuitKey(CardSuit suit)
        {
            string key;
            switch (suit)
            {
                case CardSuit.Clubs:
                    key = ClubsKey;
                    break;
                case CardSuit.Diamonds:
                    key = DiamondsKey;
                    break;
                case CardSuit.Hearts:
                    key = HeartsKey;
                    break;
                default:
                    key = SpadesKey;
                    break;
            }

            return key;
        }
    }
}
