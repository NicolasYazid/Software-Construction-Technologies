using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Services
{
    // It stands in for the services of the server, which the client does not reach yet.
    // That way every screen can be walked and reviewed with realistic data.
    // The screens take their data only from here, which leaves a single place to replace once the server answers.
    public sealed class SampleDataService
    {
        private const string PlayerName = "Player A";
        private const string GuestName = "Guest_4091";
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

        // Its values reproduce the prototype screen P11.
        public LobbySnapshot GetLobby()
        {
            LobbySnapshot lobby = new LobbySnapshot();
            lobby.PlayerName = PlayerName;
            lobby.ChatEntries = new List<ChatEntry>
            {
                CreateMessage("Player A", new TimeSpan(2, 33, 0), "¿Alguien para una partida rápida?"),
                CreateNotice(ChallengeNoticeKind.Sent, "Player D", new TimeSpan(2, 33, 0)),
                CreateMessage("Player A", new TimeSpan(2, 34, 0), "Voy por la revancha."),
                CreateCensoredMessage("Player H", new TimeSpan(2, 36, 0)),
                CreateMessage("Player G", new TimeSpan(2, 36, 0), "gg wp"),
                CreateNotice(ChallengeNoticeKind.Declined, "Player D", new TimeSpan(2, 40, 0)),
                CreateMessage("Player Z", new TimeSpan(4, 33, 0), "@Player A gl hf!"),
                CreateMessage("Player A", new TimeSpan(4, 34, 0), "You too."),
                CreateNotice(ChallengeNoticeKind.Received, "Player Z", new TimeSpan(4, 35, 0))
            };
            lobby.LookingToPlay = new List<LobbyPlayerViewModel>
            {
                CreatePlayer("Player B", "S", false),
                CreatePlayer("Player C", "A", false),
                CreatePlayer("Player D", "C", false),
                CreatePlayer("Player F", "D", false),
                CreatePlayer("Player K", "B", false)
            };
            lobby.Online = new List<LobbyPlayerViewModel>();
            lobby.MatchesInProgress = new List<MatchPair>
            {
                CreatePair(CreatePlayer("Player Z", "A", false), CreatePlayer("Player X", "S", false))
            };
            lobby.PlayersInMatchCount = 2;
            lobby.Unavailable = CreateUnavailablePlayers();
            // The groups of friends are observable so that the panel drops a player when the friendship ends (CU-16).
            lobby.FriendsOnline = new ObservableCollection<LobbyPlayerViewModel>
            {
                CreatePlayer("Friend A", "S", true),
                CreatePlayer("Friend B", "A", true),
                CreatePlayer("Friend C", "B", true),
                CreatePlayer("Friend D", "C", true)
            };
            lobby.FriendsInMatch = new List<MatchPair>
            {
                CreatePair(CreatePlayer("Friend Z", "S", true), CreatePlayer("Friend X", "A", true))
            };
            lobby.FriendsInMatchCount = 2;
            lobby.FriendsUnavailable = new ObservableCollection<LobbyPlayerViewModel>
            {
                CreatePlayer("Friend Q", "D", true),
                CreatePlayer("Friend R", "F", true)
            };

            return lobby;
        }

        // Its values reproduce the prototype screen P16.
        // A guest has no friends, so the groups of friends come empty.
        public LobbySnapshot GetGuestLobby()
        {
            LobbySnapshot lobby = new LobbySnapshot();
            lobby.PlayerName = GuestName;
            lobby.ChatEntries = new List<ChatEntry>
            {
                CreateMessage("Player A", new TimeSpan(2, 33, 0), "¿Alguien para una partida rápida?"),
                CreateMessage("Player C", new TimeSpan(2, 33, 0), "Yo, en cuanto termine esta."),
                CreateMessage("Player A", new TimeSpan(2, 34, 0), "Voy por la revancha."),
                CreateCensoredMessage("Player H", new TimeSpan(2, 36, 0)),
                CreateMessage("Player G", new TimeSpan(2, 36, 0), "gg wp"),
                CreateNotice(ChallengeNoticeKind.Declined, "Player D", new TimeSpan(2, 40, 0)),
                CreateMessage("Player Z", new TimeSpan(4, 33, 0), "@Player A gl hf!"),
                CreateMessage("Player A", new TimeSpan(4, 34, 0), "You too.")
            };
            lobby.LookingToPlay = new List<LobbyPlayerViewModel>();
            lobby.Online = new List<LobbyPlayerViewModel>
            {
                CreatePlayer("Player A", "S", false),
                CreatePlayer("Player B", "S", false),
                CreatePlayer("Player C", "A", false),
                CreatePlayer("Player D", "C", false),
                CreatePlayer("Player F", "D", false)
            };
            lobby.MatchesInProgress = new List<MatchPair>
            {
                CreatePair(CreatePlayer("Player Z", "A", false), CreatePlayer("Player X", "S", false))
            };
            lobby.PlayersInMatchCount = 2;
            lobby.Unavailable = CreateUnavailablePlayers();
            lobby.FriendsOnline = new List<LobbyPlayerViewModel>();
            lobby.FriendsInMatch = new List<MatchPair>();
            lobby.FriendsUnavailable = new List<LobbyPlayerViewModel>();

            return lobby;
        }

        // Its values reproduce the prototype screen P12.
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

        // Its values reproduce the prototype screen P13.
        public IList<SanctionViewModel> GetSanctions()
        {
            // The reasons of a ban are a catalogue of the database, translated there (BD-09).
            // Until the database can be reached, the closest names the dictionary already carries stand in for them.
            LocalizationProvider localization = LocalizationProvider.Instance;

            return new List<SanctionViewModel>
            {
                CreateActiveSanction(localization.GetText(HarassmentReasonKey), new TimeSpan(47, 59, 59)),
                CreateServedSanction(localization.GetText(OffensiveLanguageReasonKey), new DateTime(2026, 8, 2)),
                CreateServedSanction(localization.GetText(SpamReasonKey), new DateTime(2026, 7, 15))
            };
        }

        // Its values reproduce the prototype screen P14.
        public AccountSettings GetAccountSettings()
        {
            AccountSettings settings = new AccountSettings();
            settings.Email = "playera@correo.com";
            settings.IsTwoStepEnabled = true;
            settings.MasterVolume = 70;
            settings.IsChatFilterEnabled = false;
            settings.LinkedAccounts = new List<LinkedAccount>
            {
                CreateLinkedAccount("Discord", true),
                CreateLinkedAccount("X", false)
            };

            return settings;
        }

        // Its values reproduce the prototype screens P21 and P15.
        public PlayerProfile GetOwnProfile()
        {
            PlayerProfile profile = CreateProfile(PlayerName, "S", "#PLA-1024");
            profile.Bio = "Listo para una partida rápida.";
            profile.SocialLinks.Add(CreateSocialLink("Discord", "discord.gg/playerA"));
            profile.SocialLinks.Add(CreateSocialLink("X", "x.com/playerA"));
            profile.IsOwnProfile = true;

            return profile;
        }

        // Its values reproduce the prototype screen P21.
        public PlayerProfile GetPlayerProfile(LobbyPlayerViewModel player)
        {
            PlayerProfile profile = CreateProfile(player.Username, player.RankName, "#PLZ-4821");
            profile.Bio = "Siempre listo para una partida. Juego casi siempre por las tardes.";
            profile.SocialLinks.Add(CreateSocialLink("Twitch", "twitch.tv/playerz"));
            profile.IsFriend = player.IsFriend;
            profile.HasPendingRequest = player.HasPendingRequest;

            return profile;
        }

        public IList<string> GetPlatforms()
        {
            return new List<string> { "Discord", "Instagram", "Twitch", "X", "YouTube" };
        }

        // Its values reproduce the prototype screen P19.
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

        // Its values reproduce the prototype screen P20.
        // It shows the moment the player decides whether to take the card turned up.
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

        // Its values reproduce the prototype screen P20.
        public HandResult GetHandResult()
        {
            HandResult result = new HandResult();
            result.WinnerName = OpponentName;
            result.KnockerName = OpponentName;
            result.KnockerDeadwood = 1;
            result.DefenderDeadwood = 15;
            result.PointsAwarded = 14;
            result.KnockerMelds = new List<Meld>
            {
                CreateMeld(
                    MeldKind.Set,
                    CreateCard(CardRank.Ten, CardSuit.Spades),
                    CreateCard(CardRank.Ten, CardSuit.Diamonds),
                    CreateCard(CardRank.Ten, CardSuit.Hearts)),
                CreateMeld(
                    MeldKind.Run,
                    CreateCard(CardRank.Six, CardSuit.Spades),
                    CreateCard(CardRank.Seven, CardSuit.Spades),
                    CreateCard(CardRank.Eight, CardSuit.Spades)),
                CreateMeld(
                    MeldKind.Run,
                    CreateCard(CardRank.Four, CardSuit.Hearts),
                    CreateCard(CardRank.Five, CardSuit.Hearts),
                    CreateCard(CardRank.Six, CardSuit.Hearts)),
                CreateMeld(MeldKind.Unmatched, CreateCard(CardRank.Ace, CardSuit.Hearts))
            };
            result.DefenderMelds = new List<Meld>
            {
                CreateMeld(
                    MeldKind.Set,
                    CreateCard(CardRank.Seven, CardSuit.Diamonds),
                    CreateCard(CardRank.Seven, CardSuit.Hearts),
                    CreateCard(CardRank.Seven, CardSuit.Clubs)),
                CreateMeld(
                    MeldKind.Run,
                    CreateCard(CardRank.Jack, CardSuit.Spades),
                    CreateCard(CardRank.Queen, CardSuit.Spades),
                    CreateCard(CardRank.King, CardSuit.Spades)),
                CreateMeld(
                    MeldKind.Unmatched,
                    CreateCard(CardRank.Six, CardSuit.Diamonds),
                    CreateCard(CardRank.Six, CardSuit.Clubs),
                    CreateCard(CardRank.Two, CardSuit.Diamonds),
                    CreateCard(CardRank.Ace, CardSuit.Spades))
            };
            result.PlayerScore = 0;
            result.OpponentScore = 14;

            return result;
        }

        // Its values reproduce the prototype screen P20.
        public MatchResult GetMatchResult(MatchEndReason endReason)
        {
            MatchResult result = new MatchResult();
            result.EndReason = endReason;
            switch (endReason)
            {
                case MatchEndReason.OpponentForfeited:
                    result.PlayerScore = 64;
                    result.OpponentScore = 43;
                    break;
                case MatchEndReason.OpponentReachedTarget:
                    result.PlayerScore = 85;
                    result.OpponentScore = TargetScore;
                    break;
                default:
                    result.PlayerScore = TargetScore;
                    result.OpponentScore = 43;
                    break;
            }

            return result;
        }

        private static PlayerProfile CreateProfile(string username, string rankName, string publicTag)
        {
            PlayerProfile profile = new PlayerProfile();
            profile.Username = username;
            profile.RankName = rankName;
            profile.PublicTag = publicTag;
            profile.SocialLinks = new List<SocialLink>();
            profile.MatchesPlayed = 124;
            profile.WinRate = 0.58;
            profile.Score = 1450;

            return profile;
        }

        private static SocialLink CreateSocialLink(string platformName, string url)
        {
            SocialLink link = new SocialLink();
            link.PlatformName = platformName;
            link.Url = url;

            return link;
        }

        private static LinkedAccount CreateLinkedAccount(string platformName, bool isLinked)
        {
            LinkedAccount account = new LinkedAccount();
            account.PlatformName = platformName;
            account.IsLinked = isLinked;

            return account;
        }

        private static Card CreateCard(CardRank rank, CardSuit suit)
        {
            Card card = new Card();
            card.Rank = rank;
            card.Suit = suit;

            return card;
        }

        private static Meld CreateMeld(MeldKind kind, params Card[] cards)
        {
            Meld meld = new Meld();
            meld.Kind = kind;
            meld.Cards = new List<Card>(cards);

            return meld;
        }

        private static MatchLogEntry CreateLogEntry(string playerName, TimeSpan timeOfDay, MatchLogKind kind)
        {
            MatchLogEntry entry = new MatchLogEntry();
            entry.PlayerName = playerName;
            entry.OccurredAt = DateTime.Today.Add(timeOfDay);
            entry.Kind = kind;

            return entry;
        }

        private static List<LobbyPlayerViewModel> CreateUnavailablePlayers()
        {
            return new List<LobbyPlayerViewModel>
            {
                CreatePlayer("Player Q", "S", false),
                CreatePlayer("Player R", "A", false),
                CreatePlayer("Player M", "B", false),
                CreatePlayer("Player N", "C", false),
                CreatePlayer("Player O", "D", false),
                CreatePlayer("Player P", "S", false)
            };
        }

        private static LobbyPlayerViewModel CreatePlayer(string username, string rankName, bool isFriend)
        {
            LobbyPlayerViewModel player = new LobbyPlayerViewModel();
            player.Username = username;
            player.RankName = rankName;
            player.IsFriend = isFriend;

            return player;
        }

        private static MatchPair CreatePair(LobbyPlayerViewModel firstPlayer, LobbyPlayerViewModel secondPlayer)
        {
            MatchPair pair = new MatchPair();
            pair.FirstPlayer = firstPlayer;
            pair.SecondPlayer = secondPlayer;

            return pair;
        }

        private static ChatMessage CreateMessage(string authorName, TimeSpan timeOfDay, string content)
        {
            ChatMessage message = new ChatMessage();
            message.AuthorName = authorName;
            message.Content = content;
            message.SentAt = DateTime.Today.Add(timeOfDay);

            return message;
        }

        private static ChatMessage CreateCensoredMessage(string authorName, TimeSpan timeOfDay)
        {
            ChatMessage message = CreateMessage(authorName, timeOfDay, string.Empty);
            message.IsCensored = true;

            return message;
        }

        private static ChallengeNotice CreateNotice(ChallengeNoticeKind kind, string playerName, TimeSpan timeOfDay)
        {
            ChallengeNotice notice = new ChallengeNotice();
            notice.Kind = kind;
            notice.PlayerName = playerName;
            notice.SentAt = DateTime.Today.Add(timeOfDay);

            return notice;
        }

        private static FriendRequest CreateRequest(string senderName, int minutesAgo)
        {
            FriendRequest request = new FriendRequest();
            request.SenderName = senderName;
            request.CreatedAt = DateTime.Now.AddMinutes(-minutesAgo);

            return request;
        }

        private static SanctionViewModel CreateActiveSanction(string reasonName, TimeSpan remainingTime)
        {
            SanctionViewModel sanction = new SanctionViewModel();
            sanction.ReasonName = reasonName;
            sanction.IsActive = true;
            sanction.RemainingTime = remainingTime;

            return sanction;
        }

        private static SanctionViewModel CreateServedSanction(string reasonName, DateTime servedAt)
        {
            SanctionViewModel sanction = new SanctionViewModel();
            sanction.ReasonName = reasonName;
            sanction.ServedAt = servedAt;

            return sanction;
        }
    }
}
