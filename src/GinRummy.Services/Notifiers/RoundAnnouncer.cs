using System;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Contracts.DataContracts.Gameplay;
using GinRummy.Contracts.Services.Gameplay;
using GinRummy.Services.Mappers.Gameplay;

namespace GinRummy.Services.Notifiers
{
    public class RoundAnnouncer : IRoundAnnouncer
    {
        private readonly OnlinePlayers<IGameCallback> _onlinePlayers;

        public RoundAnnouncer(OnlinePlayers<IGameCallback> onlinePlayers)
        {
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public void AnnounceRoundStarted(int playerId, ActiveRound round)
        {
            RoundDetails details = GameMapper.MapToRoundDetails(round, playerId);
            EnqueueNotice(playerId, callback => callback.OnRoundStarted(details));
        }

        public void AnnounceRoundChanged(int playerId, ActiveRound round)
        {
            RoundDetails details = GameMapper.MapToRoundDetails(round, playerId);
            EnqueueNotice(playerId, callback => callback.OnRoundChanged(details));
        }

        public void AnnounceRoundFinished(int playerId, ActiveRound round)
        {
            RoundSummary summary = GameMapper.MapToRoundSummary(round, playerId);
            EnqueueNotice(playerId, callback => callback.OnRoundFinished(summary));
        }

        public void AnnounceOpponentLeft(int playerId)
        {
            EnqueueNotice(playerId, callback => callback.OnOpponentLeft());
        }

        private void EnqueueNotice(int playerId, Action<IGameCallback> notice)
        {
            WcfCallbackDispatcher<IGameCallback> playerDispatcher = _onlinePlayers.FindDispatcherOfPlayer(playerId);
            if (playerDispatcher != null)
            {
                playerDispatcher.EnqueueNotice(notice);
            }
        }
    }
}
