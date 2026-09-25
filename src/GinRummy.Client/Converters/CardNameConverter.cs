using System;
using System.Globalization;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;

namespace GinRummy.Client.Converters
{
    /// <summary>
    /// Builds the spoken name of a card, the one assistive technologies read, from the rank and
    /// the name of the suit in the active language. The symbol of the suit is a drawing and
    /// never takes part in it. The first value of the binding is the card; the second is the
    /// active culture, bound to the localization provider so that the name is rebuilt when the
    /// culture changes.
    /// </summary>
    public sealed class CardNameConverter : IMultiValueConverter
    {
        private const string NameFormatKey = "Card_A11yFormat";
        private const string ClubsKey = "Card_SuitClubs";
        private const string DiamondsKey = "Card_SuitDiamonds";
        private const string HeartsKey = "Card_SuitHearts";
        private const string SpadesKey = "Card_SuitSpades";

        /// <summary>
        /// Returns the spoken name of the card in the active culture.
        /// </summary>
        /// <param name="values">The card followed by the active culture.</param>
        /// <param name="targetType">Type requested by the binding.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Culture supplied by the binding, which is not the active
        /// one.</param>
        /// <returns>The name of the card, or an empty string while there is no card.</returns>
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

        /// <summary>
        /// Not supported: the name of a card is only shown, never written back.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetTypes">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns: the operation is not supported.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("The name of a card is never written back.");
        }

        // Shared with the log of the match, whose entries name the card they speak of. The order
        // of the rank and the suit comes from the format, since it changes between languages.
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
