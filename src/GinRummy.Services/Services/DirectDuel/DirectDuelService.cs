using System;
using System.ServiceModel;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Contracts.Faults.Shared;
using GinRummy.Contracts.Services.DirectDuel;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Notifiers;
using GinRummy.Services.Services.Shared;

namespace GinRummy.Services.Services.DirectDuel
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class DirectDuelService : IDirectDuelService
    {
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IChallengeUseCase _challengeUseCase;
        private readonly OnlinePlayers<IDirectDuelCallback> _onlinePlayers;
        private int _subscribedPlayerId;
        private IDirectDuelCallback _subscribedCallback;

        public DirectDuelService(
            AccountAccessPolicy accessPolicy,
            IChallengeUseCase challengeUseCase,
            OnlinePlayers<IDirectDuelCallback> onlinePlayers)
        {
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _challengeUseCase = challengeUseCase ?? throw new ArgumentNullException(nameof(challengeUseCase));
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public void SubscribeToDirectDuel(string accessToken)
        {
            RunAsPlayer(accessToken, SubscribePlayer);
        }

        public void ChallengePlayer(string accessToken, int challengedPublicTag)
        {
            RunAsPlayer(accessToken, playerId => _challengeUseCase.Challenge(playerId, challengedPublicTag));
        }

        public void CancelChallenge(string accessToken)
        {
            RunAsPlayer(accessToken, _challengeUseCase.Cancel);
        }

        public void AcceptChallenge(string accessToken, int challengerPublicTag)
        {
            RunAsPlayer(accessToken, playerId => _challengeUseCase.Accept(playerId, challengerPublicTag));
        }

        public void DeclineChallenge(string accessToken, int challengerPublicTag)
        {
            RunAsPlayer(accessToken, playerId => _challengeUseCase.Decline(playerId, challengerPublicTag));
        }

        public void UnsubscribeFromDirectDuel()
        {
            LeaveChallenges();
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
            catch (PlayerUnavailableException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.PlayerUnavailable);
            }
            catch (ChallengeNotFoundException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.ChallengeNotFound);
            }
        }

        private void SubscribePlayer(int playerId)
        {
            OperationContext context = OperationContext.Current;
            IDirectDuelCallback callback = context.GetCallbackChannel<IDirectDuelCallback>();
            _onlinePlayers.AddPlayer(playerId, callback);
            _subscribedPlayerId = playerId;
            _subscribedCallback = callback;
            context.Channel.Closed += OnChannelClosed;
            context.Channel.Faulted += OnChannelFaulted;
        }

        private void LeaveChallenges()
        {
            IDirectDuelCallback callbackToRemove = _subscribedCallback;
            if (callbackToRemove == null)
            {
                return;
            }

            _subscribedCallback = null;
            _onlinePlayers.RemovePlayer(_subscribedPlayerId, callbackToRemove);
            _challengeUseCase.Cancel(_subscribedPlayerId);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            LeaveChallenges();
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            LeaveChallenges();
        }
    }
}
