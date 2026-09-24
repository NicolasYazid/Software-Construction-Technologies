using System;
using System.Collections.Generic;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;

namespace GinRummy.Client.Services
{
    /// <summary>
    /// Supplies the lobby and the screens it opens with the content the prototype draws. It
    /// stands in for the services of the server, which the client does not reach yet, so that
    /// every screen can be walked and reviewed with realistic data. The screens take their data
    /// only from here, which leaves a single place to replace once the server answers.
    /// </summary>
    public sealed class SampleDataService
    {
        private const string PlayerName = "Player A";
        private const string GuestName = "Guest_4091";
        private const string OwnRankName = "S";
        private const int OwnWins = 72;
        private const int OwnLosses = 52;
        private const int OwnGlobalPosition = 24;
        private const int CardsPerHand = 10;
        private const int KnockThreshold = 10;
        private const int GinBonus = 25;
        private const int UndercutBonus = 25;
        private const int StockCardsToVoid = 2;
        private const int TargetScore = 100;

        /// <summary>
        /// Builds the lobby a registered player finds when entering (P11).
        /// </summary>
        /// <returns>The chat so far and the panel of players with its groups.</returns>
        public LobbySnapshotDto GetLobby()
        {
            LobbySnapshotDto lobby = new LobbySnapshotDto();
            lobby.PlayerName = PlayerName;
            lobby.ChatEntries = new List<ChatEntryDto>
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
            lobby.LookingToPlay = new List<LobbyPlayerDto>
            {
                CreatePlayer("Player B", "S", false),
                CreatePlayer("Player C", "A", false),
                CreatePlayer("Player D", "C", false),
                CreatePlayer("Player F", "D", false),
                CreatePlayer("Player K", "B", false)
            };
            lobby.Online = new List<LobbyPlayerDto>();
            lobby.MatchesInProgress = new List<MatchPairDto>
            {
                CreatePair(CreatePlayer("Player Z", "A", false), CreatePlayer("Player X", "S", false))
            };
            lobby.PlayersInMatchCount = 2;
            lobby.Unavailable = CreateUnavailablePlayers();
            lobby.FriendsOnline = new List<LobbyPlayerDto>
            {
                CreatePlayer("Friend A", "S", true),
                CreatePlayer("Friend B", "A", true),
                CreatePlayer("Friend C", "B", true),
                CreatePlayer("Friend D", "C", true)
            };
            lobby.FriendsInMatch = new List<MatchPairDto>
            {
                CreatePair(CreatePlayer("Friend Z", "S", true), CreatePlayer("Friend X", "A", true))
            };
            lobby.FriendsInMatchCount = 2;
            lobby.FriendsUnavailable = new List<LobbyPlayerDto>
            {
                CreatePlayer("Friend Q", "D", true),
                CreatePlayer("Friend R", "F", true)
            };

            return lobby;
        }

        /// <summary>
        /// Builds the lobby a guest finds when entering (P16). A guest has no friends, so the
        /// groups of friends come empty.
        /// </summary>
        /// <returns>The chat so far and the panel of players with its groups.</returns>
        public LobbySnapshotDto GetGuestLobby()
        {
            LobbySnapshotDto lobby = new LobbySnapshotDto();
            lobby.PlayerName = GuestName;
            lobby.ChatEntries = new List<ChatEntryDto>
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
            lobby.LookingToPlay = new List<LobbyPlayerDto>();
            lobby.Online = new List<LobbyPlayerDto>
            {
                CreatePlayer("Player A", "S", false),
                CreatePlayer("Player B", "S", false),
                CreatePlayer("Player C", "A", false),
                CreatePlayer("Player D", "C", false),
                CreatePlayer("Player F", "D", false)
            };
            lobby.MatchesInProgress = new List<MatchPairDto>
            {
                CreatePair(CreatePlayer("Player Z", "A", false), CreatePlayer("Player X", "S", false))
            };
            lobby.PlayersInMatchCount = 2;
            lobby.Unavailable = CreateUnavailablePlayers();
            lobby.FriendsOnline = new List<LobbyPlayerDto>();
            lobby.FriendsInMatch = new List<MatchPairDto>();
            lobby.FriendsUnavailable = new List<LobbyPlayerDto>();

            return lobby;
        }

        /// <summary>
        /// Lists the friend requests that wait for the answer of the player (P12).
        /// </summary>
        /// <returns>The requests, from the most recent to the oldest.</returns>
        public IList<FriendRequestDto> GetFriendRequests()
        {
            return new List<FriendRequestDto>
            {
                CreateRequest("Player Z", 5),
                CreateRequest("Player Y", 30),
                CreateRequest("Player W", 90),
                CreateRequest("Player V", 240),
                CreateRequest("Player U", 600)
            };
        }

        /// <summary>
        /// Lists the sanctions of the player (P13), with the name of each reason already in the
        /// active language.
        /// </summary>
        /// <returns>The sanctions, from the most recent to the oldest.</returns>
        public IList<SanctionDto> GetSanctions()
        {
            // The reasons of a ban are a catalogue of the database, translated there (BD-09).
            // Until it can be reached, the closest names the dictionary already carries stand
            // in for them.
            LocalizationProvider localization = LocalizationProvider.Instance;

            return new List<SanctionDto>
            {
                CreateActiveSanction(localization.GetText("Reason_HarassmentOrThreats"), new TimeSpan(47, 59, 59)),
                CreateServedSanction(localization.GetText("Reason_OffensiveLanguage"), new DateTime(2026, 8, 2)),
                CreateServedSanction(localization.GetText("Reason_Spam"), new DateTime(2026, 7, 15))
            };
        }

        /// <summary>
        /// Builds the settings of the account of the player (P14).
        /// </summary>
        /// <returns>The settings as the profile panel shows them.</returns>
        public AccountSettingsDto GetAccountSettings()
        {
            AccountSettingsDto settings = new AccountSettingsDto();
            settings.Email = "playera@correo.com";
            settings.IsTwoStepEnabled = true;
            settings.MasterVolume = 70;
            settings.IsChatFilterEnabled = false;
            settings.LinkedAccounts = new List<LinkedAccountDto>
            {
                CreateLinkedAccount("Discord", true),
                CreateLinkedAccount("X", false)
            };

            return settings;
        }

        /// <summary>
        /// Builds the profile of the player who looks at it (P21 and P15).
        /// </summary>
        /// <returns>The profile, marked as the own one.</returns>
        public PlayerProfileDto GetOwnProfile()
        {
            PlayerProfileDto profile = CreateProfile(PlayerName, "S", "#PLA-1024");
            profile.Bio = "Listo para una partida rápida.";
            profile.SocialLinks.Add(CreateSocialLink("Discord", "discord.gg/playerA"));
            profile.SocialLinks.Add(CreateSocialLink("X", "x.com/playerA"));
            profile.IsOwnProfile = true;

            return profile;
        }

        /// <summary>
        /// Builds the profile of another player of the lobby (P21).
        /// </summary>
        /// <param name="player">Player chosen in the panel of the lobby.</param>
        /// <returns>The profile, with the relation the panel already knows.</returns>
        public PlayerProfileDto GetPlayerProfile(LobbyPlayerDto player)
        {
            PlayerProfileDto profile = CreateProfile(player.Username, player.RankName, "#PLZ-4821");
            profile.Bio = "Siempre listo para una partida. Juego casi siempre por las tardes.";
            profile.SocialLinks.Add(CreateSocialLink("Twitch", "twitch.tv/playerz"));
            profile.IsFriend = player.IsFriend;

            return profile;
        }

        /// <summary>
        /// Lists the platforms a social link can point to, as their catalogue names them.
        /// </summary>
        /// <returns>The names of the platforms.</returns>
        public IList<string> GetPlatforms()
        {
            return new List<string> { "Discord", "Instagram", "Twitch", "X", "YouTube" };
        }

        /// <summary>
        /// Builds the leaderboard of every registered player (P18). The player stands beyond
        /// the rows the table holds, as in CU-19 FA-03.
        /// </summary>
        /// <returns>The first places and the row of the player.</returns>
        public LeaderboardDto GetGlobalLeaderboard()
        {
            LeaderboardDto leaderboard = new LeaderboardDto();
            leaderboard.Entries = new List<RankingEntryDto>
            {
                CreateEntry(CreatePlayer("Player Z", "S", false), 182, 41),
                CreateEntry(CreatePlayer("Player X", "A", false), 176, 52),
                CreateEntry(CreatePlayer("Player C", "A", false), 164, 58),
                CreateEntry(CreatePlayer("Player Y", "A", false), 151, 63),
                CreateEntry(CreatePlayer("Player G", "A", false), 148, 70),
                CreateEntry(CreatePlayer("Player H", "B", false), 139, 74),
                CreateEntry(CreatePlayer("Player Q", "B", false), 131, 80),
                CreateEntry(CreatePlayer("Player R", "C", false), 126, 85),
                CreateEntry(CreatePlayer("Player M", "C", false), 118, 90),
                CreateEntry(CreatePlayer("Player N", "C", false), 112, 96)
            };
            NumberEntries(leaderboard.Entries);
            leaderboard.OwnEntry = CreateOwnEntry();
            leaderboard.OwnEntry.Position = OwnGlobalPosition;

            return leaderboard;
        }

        /// <summary>
        /// Builds the leaderboard of the player and their friends (P18, CU-19 FA-01).
        /// </summary>
        /// <returns>The friends and the player, with the row of the player among them.</returns>
        public LeaderboardDto GetFriendsLeaderboard()
        {
            LeaderboardDto leaderboard = new LeaderboardDto();
            leaderboard.OwnEntry = CreateOwnEntry();
            leaderboard.Entries = new List<RankingEntryDto>
            {
                CreateEntry(CreatePlayer("Friend A", "S", true), 158, 49),
                CreateEntry(CreatePlayer("Friend B", "A", true), 133, 61),
                CreateEntry(CreatePlayer("Friend C", "B", true), 97, 58),
                leaderboard.OwnEntry,
                CreateEntry(CreatePlayer("Friend D", "C", true), 64, 71),
                CreateEntry(CreatePlayer("Friend Q", "D", true), 51, 44),
                CreateEntry(CreatePlayer("Friend R", "F", true), 38, 40)
            };
            NumberEntries(leaderboard.Entries);

            return leaderboard;
        }

        /// <summary>
        /// Builds the values of the game the house rules quote (P19). They come from the
        /// configuration of the game, which the client does not read yet.
        /// </summary>
        /// <returns>The values of the game.</returns>
        public GameRulesDto GetGameRules()
        {
            GameRulesDto rules = new GameRulesDto();
            rules.CardsPerHand = CardsPerHand;
            rules.KnockThreshold = KnockThreshold;
            rules.GinBonus = GinBonus;
            rules.UndercutBonus = UndercutBonus;
            rules.StockCardsToVoid = StockCardsToVoid;
            rules.TargetScore = TargetScore;

            return rules;
        }

        private static PlayerProfileDto CreateProfile(string username, string rankName, string publicTag)
        {
            PlayerProfileDto profile = new PlayerProfileDto();
            profile.Username = username;
            profile.RankName = rankName;
            profile.PublicTag = publicTag;
            profile.IsOnline = true;
            profile.SocialLinks = new List<SocialLinkDto>();
            profile.MatchesPlayed = 124;
            profile.WinRate = 0.58;
            profile.Score = 1450;

            return profile;
        }

        private static SocialLinkDto CreateSocialLink(string platformName, string url)
        {
            SocialLinkDto link = new SocialLinkDto();
            link.PlatformName = platformName;
            link.Url = url;

            return link;
        }

        private static LinkedAccountDto CreateLinkedAccount(string platformName, bool isLinked)
        {
            LinkedAccountDto account = new LinkedAccountDto();
            account.PlatformName = platformName;
            account.IsLinked = isLinked;

            return account;
        }

        private static RankingEntryDto CreateEntry(LobbyPlayerDto player, int wins, int losses)
        {
            RankingEntryDto entry = new RankingEntryDto();
            entry.Username = player.Username;
            entry.RankName = player.RankName;
            entry.Wins = wins;
            entry.Losses = losses;
            entry.WinRate = (double)wins / (wins + losses);

            return entry;
        }

        private static RankingEntryDto CreateOwnEntry()
        {
            RankingEntryDto entry = CreateEntry(CreatePlayer(PlayerName, OwnRankName, false), OwnWins, OwnLosses);
            entry.IsOwnEntry = true;

            return entry;
        }

        private static void NumberEntries(IList<RankingEntryDto> entries)
        {
            for (int index = 0; index < entries.Count; index++)
            {
                entries[index].Position = index + 1;
            }
        }

        private static List<LobbyPlayerDto> CreateUnavailablePlayers()
        {
            return new List<LobbyPlayerDto>
            {
                CreatePlayer("Player Q", "S", false),
                CreatePlayer("Player R", "A", false),
                CreatePlayer("Player M", "B", false),
                CreatePlayer("Player N", "C", false),
                CreatePlayer("Player O", "D", false),
                CreatePlayer("Player P", "S", false)
            };
        }

        private static LobbyPlayerDto CreatePlayer(string username, string rankName, bool isFriend)
        {
            LobbyPlayerDto player = new LobbyPlayerDto();
            player.Username = username;
            player.RankName = rankName;
            player.IsFriend = isFriend;

            return player;
        }

        private static MatchPairDto CreatePair(LobbyPlayerDto firstPlayer, LobbyPlayerDto secondPlayer)
        {
            MatchPairDto pair = new MatchPairDto();
            pair.FirstPlayer = firstPlayer;
            pair.SecondPlayer = secondPlayer;

            return pair;
        }

        private static ChatMessageDto CreateMessage(string authorName, TimeSpan timeOfDay, string content)
        {
            ChatMessageDto message = new ChatMessageDto();
            message.AuthorName = authorName;
            message.Content = content;
            message.SentAt = DateTime.Today.Add(timeOfDay);

            return message;
        }

        private static ChatMessageDto CreateCensoredMessage(string authorName, TimeSpan timeOfDay)
        {
            ChatMessageDto message = CreateMessage(authorName, timeOfDay, string.Empty);
            message.IsCensored = true;

            return message;
        }

        private static ChallengeNoticeDto CreateNotice(ChallengeNoticeKind kind, string playerName, TimeSpan timeOfDay)
        {
            ChallengeNoticeDto notice = new ChallengeNoticeDto();
            notice.Kind = kind;
            notice.PlayerName = playerName;
            notice.SentAt = DateTime.Today.Add(timeOfDay);

            return notice;
        }

        private static FriendRequestDto CreateRequest(string senderName, int minutesAgo)
        {
            FriendRequestDto request = new FriendRequestDto();
            request.SenderName = senderName;
            request.CreatedAt = DateTime.Now.AddMinutes(-minutesAgo);

            return request;
        }

        private static SanctionDto CreateActiveSanction(string reasonName, TimeSpan remainingTime)
        {
            SanctionDto sanction = new SanctionDto();
            sanction.ReasonName = reasonName;
            sanction.IsActive = true;
            sanction.RemainingTime = remainingTime;

            return sanction;
        }

        private static SanctionDto CreateServedSanction(string reasonName, DateTime servedAt)
        {
            SanctionDto sanction = new SanctionDto();
            sanction.ReasonName = reasonName;
            sanction.ServedAt = servedAt;

            return sanction;
        }
    }
}
