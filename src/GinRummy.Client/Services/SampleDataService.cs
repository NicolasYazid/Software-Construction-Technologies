using System;
using System.Collections.Generic;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Services
{
    // It stands in for the services of the server, which the client does not reach yet.
    // That way every screen can be walked and reviewed with the data the prototype screens show.
    // The screens take their data only from here, which leaves a single place to replace once the server answers.
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

        public IList<FriendRequest> GetFriendRequests()
        {
            return new List<FriendRequest>
            {
                CreateRequest("Player Z", 5),
                CreateRequest("Player Y", 30),
                CreateRequest("Player W", 90),
                CreateRequest("Player V", 240),
                CreateRequest("Player U", 600)
            };
        }

        public IList<SanctionViewModel> GetSanctions()
        {
            // The reasons of a ban are a catalogue of the database, translated there.
            // Until the database can be reached, the closest names the language resources already carry stand in for them.
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

        private static FriendRequest CreateRequest(string senderName, int minutesAgo)
        {
            return new FriendRequest(senderName, DateTime.Now.AddMinutes(-minutesAgo));
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
