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
    // Every operation takes the player from the token and never from the client.
    // A player therefore can only change their own profile (CU-28 to CU-32, PRM-02).
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

        // The name is saved even when it repeats another one, because players are told apart by their tag (CU-29).
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

        public byte[] GetOwnProfilePicture(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            string storedPath;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                storedPath = unitOfWork.PlayerAccounts.FindProfile(playerId).ProfilePicture;
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

        // Only web addresses are kept, so a link never opens something else on the computer of who reads it (CU-31).
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

        // The link is searched only among the links of the own profile (CU-32 EX-05).
        // A link that is not found was already removed, so nothing is reported (CU-32 FA-03).
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

        // Only a language of the catalog is kept, because the next log in applies it (CU-20 EX-01).
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

        // The old file is deleted only after the database stops pointing to it, so no profile is left without its file.
        private void DeleteStoredPicture(string storedPath)
        {
            if (storedPath != null)
            {
                _pictureStorage.Delete(storedPath);
            }
        }
    }
}
