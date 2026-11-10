using System;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Contracts.Services.DirectDuel;

namespace GinRummy.Services.Notifiers
{
    public class ChallengeAnnouncer : IChallengeAnnouncer
    {
        private readonly OnlinePlayers<IChallengeCallback> _onlinePlayers;

        public ChallengeAnnouncer(OnlinePlayers<IChallengeCallback> onlinePlayers)
        {
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
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

        public void AnnounceRoundReady(PendingChallenge challenge)
        {
            EnqueueNotice(challenge.Challenger.Id, callback => callback.OnRoundReady());
            EnqueueNotice(challenge.Challenged.Id, callback => callback.OnRoundReady());
        }

        private void EnqueueNotice(int playerId, Action<IChallengeCallback> notice)
        {
            WcfCallbackDispatcher<IChallengeCallback> playerDispatcher = _onlinePlayers.FindDispatcherOfPlayer(playerId);
            if (playerDispatcher != null)
            {
                playerDispatcher.EnqueueNotice(notice);
            }
        }
    }
}
