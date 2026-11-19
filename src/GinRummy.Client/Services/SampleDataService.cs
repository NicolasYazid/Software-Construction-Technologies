using System;
using System.Collections.Generic;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Services
{
    public sealed class SampleDataService : ISampleDataService
    {
        private const int CardsPerHand = 10;
        private const int KnockThreshold = 10;
        private const int GinBonus = 25;
        private const int UndercutBonus = 25;
        private const int StockCardsToVoid = 2;
        private const int TargetScore = 100;
        private const string HarassmentReasonKey = "Reason_HarassmentOrThreats";
        private const string OffensiveLanguageReasonKey = "Reason_OffensiveLanguage";
        private const string SpamReasonKey = "Reason_Spam";

        public IList<SanctionViewModel> GetSanctions()
        {
            LocalizationProvider localization = LocalizationProvider.Instance;

            return new List<SanctionViewModel>
            {
                CreateActiveSanction(localization.GetText(HarassmentReasonKey), new TimeSpan(47, 59, 59)),
                CreateServedSanction(localization.GetText(OffensiveLanguageReasonKey), new DateTime(2026, 8, 2)),
                CreateServedSanction(localization.GetText(SpamReasonKey), new DateTime(2026, 7, 15))
            };
        }

        public GameRules GetGameRules()
        {
            GameRules rules = new GameRules();
            rules.CardsPerHand = CardsPerHand;
            rules.KnockThreshold = KnockThreshold;
            rules.GinBonus = GinBonus;
            rules.UndercutBonus = UndercutBonus;
            rules.StockCardsToVoid = StockCardsToVoid;
            rules.TargetScore = TargetScore;

            return rules;
        }

        private static SanctionViewModel CreateActiveSanction(string reasonName, TimeSpan remainingTime)
        {
            return new SanctionViewModel(reasonName, remainingTime);
        }

        private static SanctionViewModel CreateServedSanction(string reasonName, DateTime servedAt)
        {
            return new SanctionViewModel(reasonName, servedAt);
        }
    }
}
