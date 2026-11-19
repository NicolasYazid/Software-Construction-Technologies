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

        public void AnnounceRoundStarted(int playerId, ActiveMatch match)
        {
            RoundDetails details = GameMapper.MapToRoundDetails(match, playerId);
            EnqueueNotice(playerId, callback => callback.OnRoundStarted(details));
        }

        public void AnnounceRoundChanged(int playerId, ActiveMatch match)
        {
            RoundDetails details = GameMapper.MapToRoundDetails(match, playerId);
            EnqueueNotice(playerId, callback => callback.OnRoundChanged(details));
        }

        public void AnnounceRoundFinished(int playerId, ActiveMatch match)
        {
            RoundSummary summary = GameMapper.MapToRoundSummary(match, playerId);
            EnqueueNotice(playerId, callback => callback.OnRoundFinished(summary));
        }

        public void AnnounceOpponentLeft(int playerId)
        {
            EnqueueNotice(playerId, callback => callback.OnOpponentLeft());
        }

        public void AnnounceOpponentDisconnected(int playerId)
        {
            EnqueueNotice(playerId, callback => callback.OnOpponentDisconnected());
        }

        public void AnnounceOpponentReconnected(int playerId)
        {
            EnqueueNotice(playerId, callback => callback.OnOpponentReconnected());
        }

        public bool IsPlayerOnline(int playerId)
        {
            return _onlinePlayers.FindDispatcherOfPlayer(playerId) != null ? true : false;
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
