using System;
using System.ServiceModel;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Contracts.DataContracts.DirectDuel;
using GinRummy.Contracts.Faults.Shared;
using GinRummy.Contracts.Services.DirectDuel;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Notifiers;
using GinRummy.Services.Services.Shared;

using RequestOutcome = GinRummy.Application.UseCases.DirectDuel.ChallengeOutcome;

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
            SubscribePlayer(IdentifyPlayer(accessToken));
        }

        public ChallengeOutcome ChallengePlayer(string accessToken, int challengedPublicTag)
        {
            int playerId = IdentifyPlayer(accessToken);

            return Answer(() => _challengeUseCase.Challenge(playerId, challengedPublicTag));
        }

        public void CancelChallenge(string accessToken)
        {
            _challengeUseCase.Cancel(IdentifyPlayer(accessToken));
        }

        public ChallengeOutcome AcceptChallenge(string accessToken, int challengerPublicTag)
        {
            int playerId = IdentifyPlayer(accessToken);

            return Answer(() => _challengeUseCase.Accept(playerId, challengerPublicTag));
        }

        public ChallengeOutcome DeclineChallenge(string accessToken, int challengerPublicTag)
        {
            int playerId = IdentifyPlayer(accessToken);

            return Answer(() => _challengeUseCase.Decline(playerId, challengerPublicTag));
        }

        public void UnsubscribeFromDirectDuel()
        {
            LeaveChallenges();
        }

        private static ChallengeOutcome Answer(Func<RequestOutcome> request)
        {
            ChallengeOutcome outcome;
            try
            {
                outcome = (ChallengeOutcome)Enum.Parse(typeof(ChallengeOutcome), request().ToString());
            }
            catch (InvalidAccessTokenException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            }
            catch (SelfChallengeException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.CannotChallengeSelf);
            }
            catch (ConcurrentChallengeException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.ConcurrentChallenge);
            }

            return outcome;
        }

        private int IdentifyPlayer(string accessToken)
        {
            AccessTokenOwner owner;
            try
            {
                owner = _accessPolicy.IdentifyByAccessToken(accessToken);
            }
            catch (InvalidAccessTokenException)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            }

            if (owner.IsGuest)
            {
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.GuestNotAllowed);
            }

            return owner.PlayerAccountId.Value;
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
