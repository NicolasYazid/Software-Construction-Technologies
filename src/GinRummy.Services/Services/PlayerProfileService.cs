using System;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Application.UseCases;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using GinRummy.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PlayerProfileService : IPlayerProfileService
    {
        private readonly IPlayerProfilePort _playerProfilePort;
        private readonly ILogger<PlayerProfileService> _logger;

        public PlayerProfileService(IPlayerProfilePort playerProfilePort, ILogger<PlayerProfileService> logger)
        {
            _playerProfilePort = playerProfilePort ?? throw new ArgumentNullException(nameof(playerProfilePort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public PlayerProfileDetails GetPlayerProfile(string accessToken, int publicTag)
        {
            ViewedPlayerProfile viewedProfile;
            try
            {
                viewedProfile = _playerProfilePort.GetPlayerProfile(accessToken, publicTag);
            }
            catch (InvalidAccessTokenException ex)
            {
                _logger.LogWarning(ex, "A client asked for a profile with an access token that is not valid.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            }

            if (viewedProfile == null)
            {
                _logger.LogInformation("No player has the public tag {PublicTag}.", publicTag);
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.PlayerNotFound);
            }

            return PlayerProfileMapper.MapToPlayerProfileDetails(viewedProfile);
        }
    }
}
