using System;
using System.Linq;

using GinRummy.Application.Ports;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    public class EditPlayerProfileUseCase : IProfileEditPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IProfilePictureStorage _pictureStorage;

        public EditPlayerProfileUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            IProfilePictureStorage pictureStorage)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _pictureStorage = pictureStorage ?? throw new ArgumentNullException(nameof(pictureStorage));
        }

        public void SaveProfileChanges(string accessToken, string username, string bio)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            string typedUsername = (username ?? string.Empty).Trim();
            TypedFieldValidator.EnsureAreTyped(typedUsername);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(typedUsername, bio);
            TypedFieldValidator.EnsureIsNotLongerThan(typedUsername, Player.UsernameMaxLength);
            TypedFieldValidator.EnsureIsNotLongerThan(bio, PlayerProfile.BioMaxLength);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                unitOfWork.Players.FindById(playerId).ChangeUsername(typedUsername);
                unitOfWork.PlayerAccounts.FindProfile(playerId).ChangeBio(bio);
                unitOfWork.Commit();
            }
        }

        public byte[] GetProfilePicture(string accessToken, int publicTag)
        {
            _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            string storedPath = null;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                Player player = unitOfWork.Players.FindByPublicTag(publicTag);
                if (player != null)
                {
                    storedPath = unitOfWork.PlayerAccounts.FindProfile(player.PlayerId).ProfilePicture;
                }
            }

            byte[] picture = null;
            if (storedPath != null)
            {
                picture = _pictureStorage.Load(storedPath);
            }

            return picture;
        }

        public bool ChangeProfilePicture(string accessToken, byte[] picture)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            string fileExtension = ProfilePictureCommon.FindFileExtension(picture);
            bool isAccepted = fileExtension != null;
            if (isAccepted)
            {
                string storedPath = _pictureStorage.Save(playerId, picture, fileExtension);
                string previousPath = UpdateProfilePicture(playerId, profile => profile.ChangeProfilePicture(storedPath));
                if (previousPath != storedPath)
                {
                    DeleteStoredPicture(previousPath);
                }
            }

            return isAccepted;
        }

        public void RemoveProfilePicture(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            string previousPath = UpdateProfilePicture(playerId, profile => profile.RemoveProfilePicture());
            DeleteStoredPicture(previousPath);
        }

        public SocialMedia LinkSocialMedia(string accessToken, string platformName, string url)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            string typedPlatformName = (platformName ?? string.Empty).Trim();
            string typedUrl = (url ?? string.Empty).Trim();
            TypedFieldValidator.EnsureAreTyped(typedPlatformName, typedUrl);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(typedPlatformName, typedUrl);
            TypedFieldValidator.EnsureIsNotLongerThan(typedPlatformName, SocialMedia.PlatformNameMaxLength);
            TypedFieldValidator.EnsureIsNotLongerThan(typedUrl, SocialMedia.UrlMaxLength);
            SocialMedia newLink = null;
            if (SocialMedia.IsWebAddress(typedUrl))
            {
                newLink = new SocialMedia(playerId, typedPlatformName, typedUrl);
                using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
                {
                    unitOfWork.PlayerAccounts.AddSocialMedia(newLink);
                    unitOfWork.Commit();
                }
            }

            return newLink;
        }

        public void UnlinkSocialMedia(string accessToken, int socialMediaId)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                SocialMedia link = unitOfWork.PlayerAccounts.FindProfile(playerId).SocialMediaLinks
                    .FirstOrDefault(found => found.SocialMediaId == socialMediaId);
                if (link != null)
                {
                    unitOfWork.PlayerAccounts.RemoveSocialMedia(link);
                    unitOfWork.Commit();
                }
            }
        }

        public bool ChangeLanguage(string accessToken, string localeCode)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            bool isSupported;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                Locale locale = unitOfWork.Locales.FindByCode(localeCode);
                isSupported = locale != null;
                if (isSupported)
                {
                    unitOfWork.Players.FindById(playerId).ChangeLocale(locale.LocaleId);
                    unitOfWork.Commit();
                }
            }

            return isSupported;
        }

        private string UpdateProfilePicture(int playerId, Action<PlayerProfile> pictureChange)
        {
            string previousPath;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerProfile profile = unitOfWork.PlayerAccounts.FindProfile(playerId);
                previousPath = profile.ProfilePicture;
                pictureChange(profile);
                unitOfWork.Commit();
            }

            return previousPath;
        }

        private void DeleteStoredPicture(string storedPath)
        {
            if (storedPath != null)
            {
                _pictureStorage.Delete(storedPath);
            }
        }
    }
}
