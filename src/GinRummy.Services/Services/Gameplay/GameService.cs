using System;
using System.ServiceModel;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Contracts.DataContracts.Gameplay;
using GinRummy.Contracts.Faults.Shared;
using GinRummy.Contracts.Services.Gameplay;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Mappers.Gameplay;
using GinRummy.Services.Notifiers;
using GinRummy.Services.Services.Shared;

namespace GinRummy.Services.Services.Gameplay
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class GameService : IGameService
    {
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IPlayRoundUseCase _playRoundUseCase;
        private readonly OnlinePlayers<IGameCallback> _onlinePlayers;
        private int _subscribedPlayerId;
        private IGameCallback _subscribedCallback;

        public GameService(
            AccountAccessPolicy accessPolicy,
            IPlayRoundUseCase playRoundUseCase,
            OnlinePlayers<IGameCallback> onlinePlayers)
        {
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _playRoundUseCase = playRoundUseCase ?? throw new ArgumentNullException(nameof(playRoundUseCase));
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public void SubscribeToGame(string accessToken)
        {
            RunAsPlayer(accessToken, SubscribePlayer);
        }

        public void TakeFirstCard(string accessToken)
        {
            RunAsPlayer(accessToken, _playRoundUseCase.TakeFirstCard);
        }

        public void PassFirstCard(string accessToken)
        {
            RunAsPlayer(accessToken, _playRoundUseCase.PassFirstCard);
        }

        public void DrawFromStock(string accessToken)
        {
            RunAsPlayer(accessToken, _playRoundUseCase.DrawFromStock);
        }

        public void DrawFromDiscardPile(string accessToken)
        {
            RunAsPlayer(accessToken, _playRoundUseCase.DrawFromDiscardPile);
        }

        public void Discard(string accessToken, PlayingCard card)
        {
            Card discardedCard = GameMapper.MapToCard(card);
            RunAsPlayer(accessToken, playerId => _playRoundUseCase.Discard(playerId, discardedCard));
        }

        public void Knock(string accessToken, PlayingCard discardedCard)
        {
            Card knockCard = GameMapper.MapToCard(discardedCard);
            RunAsPlayer(accessToken, playerId => _playRoundUseCase.Knock(playerId, knockCard));
        }

        public void ForfeitRound(string accessToken)
        {
            RunAsPlayer(accessToken, _playRoundUseCase.ForfeitRound);
        }

        public void UnsubscribeFromGame()
        {
            LeaveGame();
        }

        private void RunAsPlayer(string accessToken, Action<int> action)
        {
            try
            {
                action(_accessPolicy.IdentifyPlayerByAccessToken(accessToken));
            }
            catch (InvalidAccessTokenException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            }
            catch (InvalidMoveException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.InvalidMove);
            }
        }

        private void SubscribePlayer(int playerId)
        {
            OperationContext context = OperationContext.Current;
            IGameCallback callback = context.GetCallbackChannel<IGameCallback>();
            _onlinePlayers.AddPlayer(playerId, callback);
            _subscribedPlayerId = playerId;
            _subscribedCallback = callback;
            context.Channel.Closed += OnChannelClosed;
            context.Channel.Faulted += OnChannelFaulted;
        }

        private void LeaveGame()
        {
            IGameCallback callbackToRemove = _subscribedCallback;
            if (callbackToRemove == null)
            {
                return;
            }

            _subscribedCallback = null;
            _onlinePlayers.RemovePlayer(_subscribedPlayerId, callbackToRemove);
            _playRoundUseCase.ForfeitRound(_subscribedPlayerId);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            LeaveGame();
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            LeaveGame();
        }
    }
}
