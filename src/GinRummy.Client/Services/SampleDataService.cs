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
        private const string PlayerName = "Player A";
        private const string OpponentName = "Player Z";
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

        public GameTableSnapshot GetGameTable()
        {
            GameTableSnapshot table = new GameTableSnapshot();
            table.PlayerName = PlayerName;
            table.OpponentName = OpponentName;
            table.OpponentCardCount = CardsPerHand;
            table.StockCount = 31;
            table.DiscardTop = CreateCard(CardRank.Seven, CardSuit.Spades);
            table.Hand = new List<Card>
            {
                CreateCard(CardRank.Ten, CardSuit.Spades),
                CreateCard(CardRank.Three, CardSuit.Hearts),
                CreateCard(CardRank.Five, CardSuit.Hearts),
                CreateCard(CardRank.Three, CardSuit.Diamonds),
                CreateCard(CardRank.Four, CardSuit.Diamonds),
                CreateCard(CardRank.Eight, CardSuit.Diamonds),
                CreateCard(CardRank.Ace, CardSuit.Clubs),
                CreateCard(CardRank.Four, CardSuit.Clubs),
                CreateCard(CardRank.Seven, CardSuit.Clubs),
                CreateCard(CardRank.Eight, CardSuit.Clubs)
            };
            table.Deadwood = 53;
            table.TargetScore = TargetScore;
            table.ChatEntries = new List<ChatMessage>
            {
                CreateMessage(OpponentName, new TimeSpan(14, 2, 0), "Gl hf!"),
                CreateMessage(PlayerName, new TimeSpan(14, 3, 0), "You too."),
                CreateMessage(OpponentName, new TimeSpan(14, 7, 0), "That was a lucky draw."),
                CreateMessage(PlayerName, new TimeSpan(14, 8, 0), "Just playing the odds.")
            };
            MatchLogEntry dealt = CreateLogEntry(OpponentName, new TimeSpan(14, 1, 0), MatchLogKind.Dealt);
            dealt.Amount = CardsPerHand;
            MatchLogEntry turnedUp = CreateLogEntry(OpponentName, new TimeSpan(14, 1, 0), MatchLogKind.TurnedUp);
            turnedUp.Card = table.DiscardTop;
            table.MatchLog = new List<MatchLogEntry>
            {
                dealt,
                turnedUp,
                CreateLogEntry(OpponentName, new TimeSpan(14, 2, 0), MatchLogKind.Waiting)
            };

            return table;
        }

        private static Card CreateCard(CardRank rank, CardSuit suit)
        {
            return new Card(rank, suit);
        }

        private static MatchLogEntry CreateLogEntry(string playerName, TimeSpan timeOfDay, MatchLogKind kind)
        {
            MatchLogEntry entry = new MatchLogEntry();
            entry.PlayerName = playerName;
            entry.OccurredAt = DateTime.Today.Add(timeOfDay);
            entry.Kind = kind;

            return entry;
        }

        private static ChatMessage CreateMessage(string authorName, TimeSpan timeOfDay, string content)
        {
            return new ChatMessage(authorName, content, DateTime.Today.Add(timeOfDay));
        }

        private static ChatMessage CreateCensoredMessage(string authorName, TimeSpan timeOfDay)
        {
            ChatMessage message = CreateMessage(authorName, timeOfDay, string.Empty);
            message.Censor();

            return message;
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
