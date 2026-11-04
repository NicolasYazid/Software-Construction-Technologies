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

        // Its values reproduce the prototype screen P11, except the players and the friends.
        // Those are not simulated: the presence service sends them, so their groups start empty.
        // The groups that receive them are observable so that the panel shows a change the moment the server reports it.
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
            lobby.LookingToPlay = new List<LobbyPlayerViewModel>();
            lobby.Online = new ObservableCollection<LobbyPlayerViewModel>();
            lobby.MatchesInProgress = new List<MatchPair>();
            lobby.Unavailable = new ObservableCollection<LobbyPlayerViewModel>();
            lobby.FriendsOnline = new ObservableCollection<LobbyPlayerViewModel>();
            lobby.FriendsInMatch = new List<MatchPair>();
            lobby.FriendsUnavailable = new ObservableCollection<LobbyPlayerViewModel>();

            return lobby;
        }

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

        public PlayerProfile GetOwnProfile()
        {
            PlayerProfile profile = CreateProfile(PlayerName, "S", "#PLA-1024");
            profile.Bio = "Listo para una partida rápida.";
            profile.SocialLinks.Add(CreateSocialLink("Discord", "discord.gg/playerA"));
            profile.SocialLinks.Add(CreateSocialLink("X", "x.com/playerA"));
            profile.IsOwnProfile = true;

            return profile;
        }

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
            return new SocialLink(platformName, url);
        }

        private static LinkedAccount CreateLinkedAccount(string platformName, bool isLinked)
        {
            return new LinkedAccount(platformName, isLinked);
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
            return new MatchPair(firstPlayer, secondPlayer);
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

        private static ChallengeNotice CreateNotice(ChallengeNoticeKind kind, string playerName, TimeSpan timeOfDay)
        {
            return new ChallengeNotice(kind, playerName, DateTime.Today.Add(timeOfDay));
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
