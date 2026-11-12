using System;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Contracts.Services.DirectDuel;

namespace GinRummy.Services.Notifiers
{
    public class ChallengeAnnouncer : IChallengeAnnouncer
    {
        private readonly OnlinePlayers<IDirectDuelCallback> _onlinePlayers;
        private readonly IRoundAnnouncer _roundAnnouncer;

        public ChallengeAnnouncer(OnlinePlayers<IDirectDuelCallback> onlinePlayers, IRoundAnnouncer roundAnnouncer)
        {
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
            _roundAnnouncer = roundAnnouncer ?? throw new ArgumentNullException(nameof(roundAnnouncer));
        }

        public bool IsPlayerOnline(int playerId)
        {
            return _onlinePlayers.FindDispatcherOfPlayer(playerId) != null;
        }

        public void AnnounceChallengeReceived(PendingChallenge challenge)
        {
            string challengerName = challenge.Challenger.Name;
            int challengerPublicTag = challenge.Challenger.PublicTag;
            EnqueueNotice(challenge.Challenged.Id, callback => callback.OnChallengeReceived(challengerName, challengerPublicTag));
        }

        public void AnnounceChallengeCancelled(PendingChallenge challenge)
        {
            int challengerPublicTag = challenge.Challenger.PublicTag;
            EnqueueNotice(challenge.Challenged.Id, callback => callback.OnChallengeCancelled(challengerPublicTag));
        }

        public void AnnounceChallengeDeclined(PendingChallenge challenge)
        {
            int challengedPublicTag = challenge.Challenged.PublicTag;
            EnqueueNotice(challenge.Challenger.Id, callback => callback.OnChallengeDeclined(challengedPublicTag));
        }

        public void AnnounceRoundStarted(ActiveRound round)
        {
            lock (round.SyncRoot)
            {
                foreach (DuelPlayer player in round.Players)
                {
                    _roundAnnouncer.AnnounceRoundStarted(player.Id, round);
                }
            }
        }

        private void EnqueueNotice(int playerId, Action<IDirectDuelCallback> notice)
        {
            WcfCallbackDispatcher<IDirectDuelCallback> playerDispatcher = _onlinePlayers.FindDispatcherOfPlayer(playerId);
            if (playerDispatcher != null)
            {
                playerDispatcher.EnqueueNotice(notice);
            }
        }
    }
}
