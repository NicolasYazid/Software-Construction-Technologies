using System;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Application.UseCases;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Players;
using GinRummy.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PlayerProfileService : IPlayerProfileService
    {
        private readonly IPlayerProfilePort _playerProfilePort;
        private readonly IProfileEditPort _profileEditPort;
        private readonly ILogger<PlayerProfileService> _logger;

        public PlayerProfileService(
            IPlayerProfilePort playerProfilePort,
            IProfileEditPort profileEditPort,
            ILogger<PlayerProfileService> logger)
        {
            _playerProfilePort = playerProfilePort ?? throw new ArgumentNullException(nameof(playerProfilePort));
            _profileEditPort = profileEditPort ?? throw new ArgumentNullException(nameof(profileEditPort));
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

        public void SaveProfileChanges(string accessToken, string username, string bio)
        {
            RunProfileEdit(() => _profileEditPort.SaveProfileChanges(accessToken, username, bio));
        }

        public byte[] GetOwnProfilePicture(string accessToken)
        {
            byte[] picture = null;
            RunProfileEdit(() => picture = _profileEditPort.GetOwnProfilePicture(accessToken));

            return picture;
        }

        public void ChangeProfilePicture(string accessToken, byte[] picture)
        {
            bool isAccepted = false;
            RunProfileEdit(() => isAccepted = _profileEditPort.ChangeProfilePicture(accessToken, picture));
            if (!isAccepted)
            {
                _logger.LogInformation("A client sent a profile picture that is not an accepted image.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.InvalidImage);
            }
        }

        public void RemoveProfilePicture(string accessToken)
        {
            RunProfileEdit(() => _profileEditPort.RemoveProfilePicture(accessToken));
        }

        public SocialLinkDetails LinkSocialMedia(string accessToken, string platformName, string url)
        {
            SocialMedia newLink = null;
            RunProfileEdit(() => newLink = _profileEditPort.LinkSocialMedia(accessToken, platformName, url));
            if (newLink == null)
            {
                _logger.LogInformation("A client sent a social link whose address is not a web address.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.InvalidUrl);
            }

            return PlayerProfileMapper.MapToSocialLinkDetails(newLink);
        }

        public void UnlinkSocialMedia(string accessToken, int socialMediaId)
        {
            RunProfileEdit(() => _profileEditPort.UnlinkSocialMedia(accessToken, socialMediaId));
        }

        public void ChangeLanguage(string accessToken, string localeCode)
        {
            bool isSupported = false;
            RunProfileEdit(() => isSupported = _profileEditPort.ChangeLanguage(accessToken, localeCode));
            if (!isSupported)
            {
                _logger.LogWarning("A client asked for the language {LocaleCode}, which is not in the catalog.", localeCode);
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.UnsupportedLocale);
            }
        }

        // Every edit breaks the same rules, so they all turn each broken rule into the same fault.
        private void RunProfileEdit(Action profileEdit)
        {
            try
            {
                profileEdit();
            }
            catch (InvalidAccessTokenException ex)
            {
                _logger.LogWarning(ex, "A client tried to edit a profile with an access token that is not valid.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            }
            catch (RequiredFieldMissingException ex)
            {
                _logger.LogInformation(ex, "A profile edit arrived with an empty required field.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (FieldTooLongException ex)
            {
                _logger.LogInformation(ex, "A profile edit arrived with a field longer than its column.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.FieldTooLong);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(ex, "A profile edit arrived with characters that are not allowed.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
        }
    }
}
