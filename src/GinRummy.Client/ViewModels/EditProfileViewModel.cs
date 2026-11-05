using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.PlayerProfileServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class EditProfileViewModel : ViewModelBase
    {
        private const string MaxImageSizeLabel = "2 MB";
        private const long MaxPictureSizeInBytes = 2 * 1024 * 1024;
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string DisallowedCharactersKey = "Error_ValDisallowedCharacters";
        private const string UnsupportedImageKey = "Error_ValUnsupportedImage";
        private const string InvalidUrlKey = "Error_ValInvalidUrl";

        private readonly PlayerProfile _profile;
        private readonly Func<IPlayerProfileService> _playerProfileServiceFactory;
        private readonly ILogger<EditProfileViewModel> _logger;
        private string _bio;
        private string _socialPlatformName;
        private string _socialUrl;
        private byte[] _picture;
        private bool _isNewLinkFormOpen;
        private string _errorMessageKey;
        private bool _isUsernameChangedShown;
        private SocialLink _linkToRemove;

        // A client is created for every call because a client whose channel failed cannot be opened again.
        // The profile received is updated after every saved change, so the profile screen shows it without asking the server.
        public EditProfileViewModel(
            PlayerProfile profile,
            Func<IPlayerProfileService> playerProfileServiceFactory,
            ILogger<EditProfileViewModel> logger)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _playerProfileServiceFactory = playerProfileServiceFactory
                ?? throw new ArgumentNullException(nameof(playerProfileServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SocialLinks = new ObservableCollection<SocialLink>(profile.SocialLinks);
            Username = profile.Username;
            _bio = profile.Bio ?? string.Empty;
            _picture = profile.Picture;
            _socialPlatformName = string.Empty;
            _socialUrl = string.Empty;
            AddSocialLinkCommand = new RelayCommand(OpenNewLinkForm);
            UnlinkCommand = new RelayCommand(RequestSocialLinkRemoval);
        }

        public event EventHandler SocialLinkRemovalRequested;

        public event EventHandler<ErrorDialogKind> ErrorDialogRequested;

        public ObservableCollection<SocialLink> SocialLinks { get; private set; }
        public string Username { get; set; }
        public ICommand AddSocialLinkCommand { get; private set; }
        public ICommand UnlinkCommand { get; private set; }

        public int BioMaxLength
        {
            get { return PlayerProfile.BioMaxLength; }
        }

        public string MaxImageSizeText
        {
            get { return MaxImageSizeLabel; }
        }

        public string Bio
        {
            get { return _bio; }
            set
            {
                _bio = value ?? string.Empty;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(RemainingBioCharacters));
            }
        }

        public int RemainingBioCharacters
        {
            get { return BioMaxLength - _bio.Length; }
        }

        public string SocialPlatformName
        {
            get { return _socialPlatformName; }
            set
            {
                _socialPlatformName = value ?? string.Empty;
                RaisePropertyChanged();
            }
        }

        public string SocialUrl
        {
            get { return _socialUrl; }
            set
            {
                _socialUrl = value ?? string.Empty;
                RaisePropertyChanged();
            }
        }

        public byte[] Picture
        {
            get { return _picture; }
            private set
            {
                _picture = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNewLinkFormOpen
        {
            get { return _isNewLinkFormOpen; }
            private set
            {
                _isNewLinkFormOpen = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsAddLinkOffered));
            }
        }

        public bool IsAddLinkOffered
        {
            get { return !_isNewLinkFormOpen; }
        }

        public string ErrorMessageKey
        {
            get { return _errorMessageKey; }
            private set
            {
                _errorMessageKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasErrorMessage));
            }
        }

        public bool HasErrorMessage
        {
            get { return _errorMessageKey != null; }
        }

        public bool IsUsernameChangedShown
        {
            get { return _isUsernameChangedShown; }
            private set
            {
                _isUsernameChangedShown = value;
                RaisePropertyChanged();
            }
        }

        // A picture over the limit is refused here, because the connection would reject the message before the server reads it.
        public void ChangePicture(string accessToken, string filePath)
        {
            ErrorMessageKey = null;
            byte[] picture = ReadPicture(filePath);
            if (picture == null)
            {
                ErrorMessageKey = UnsupportedImageKey;
            }
            else if (CallProfileService(profileService => profileService.ChangeProfilePicture(accessToken, picture)))
            {
                Picture = picture;
                _profile.Picture = picture;
            }
        }

        public void RemovePicture(string accessToken)
        {
            ErrorMessageKey = null;
            if (CallProfileService(profileService => profileService.RemoveProfilePicture(accessToken)))
            {
                Picture = null;
                _profile.Picture = null;
            }
        }

        // Only the required fields are checked here, because the server validates the rest (CU-29, CU-30 and CU-31).
        public void Save(string accessToken)
        {
            IsUsernameChangedShown = false;
            ErrorMessageKey = null;
            bool isLinkComplete = (_socialPlatformName.Trim().Length > 0) && (_socialUrl.Trim().Length > 0);
            bool isComplete = !string.IsNullOrWhiteSpace(Username) && (!_isNewLinkFormOpen || isLinkComplete);
            if (!isComplete)
            {
                ErrorMessageKey = RequiredFieldKey;
            }
            else
            {
                bool isSaved = CallProfileService(profileService => profileService.SaveProfileChanges(accessToken, Username, _bio));
                if (isSaved)
                {
                    _profile.Username = Username.Trim();
                    _profile.Bio = _bio;
                }

                if (isSaved && _isNewLinkFormOpen)
                {
                    isSaved = LinkSocialMedia(accessToken);
                }

                IsUsernameChangedShown = isSaved;
            }
        }

        public void CompleteSocialLinkRemoval(bool isConfirmed, string accessToken)
        {
            int socialMediaId = _linkToRemove.SocialMediaId;
            if (isConfirmed && CallProfileService(profileService => profileService.UnlinkSocialMedia(accessToken, socialMediaId)))
            {
                SocialLinks.Remove(_linkToRemove);
                _profile.SocialLinks = new List<SocialLink>(SocialLinks);
            }

            _linkToRemove = null;
        }

        private void OpenNewLinkForm()
        {
            IsNewLinkFormOpen = true;
        }

        private void RequestSocialLinkRemoval(object parameter)
        {
            _linkToRemove = (SocialLink)parameter;
            SocialLinkRemovalRequested?.Invoke(this, EventArgs.Empty);
        }

        private bool LinkSocialMedia(string accessToken)
        {
            SocialLinkDetails newLink = null;
            bool isLinked = CallProfileService(
                profileService => newLink = profileService.LinkSocialMedia(accessToken, _socialPlatformName, _socialUrl));
            if (isLinked)
            {
                SocialLinks.Add(new SocialLink(newLink.SocialMediaId, newLink.PlatformName, newLink.Url));
                _profile.SocialLinks = new List<SocialLink>(SocialLinks);
                SocialPlatformName = string.Empty;
                SocialUrl = string.Empty;
                IsNewLinkFormOpen = false;
            }

            return isLinked;
        }

        private byte[] ReadPicture(string filePath)
        {
            byte[] picture = null;
            try
            {
                if (new FileInfo(filePath).Length <= MaxPictureSizeInBytes)
                {
                    picture = File.ReadAllBytes(filePath);
                }
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "The chosen profile picture could not be read.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "The chosen profile picture could not be opened.");
            }

            return picture;
        }

        // The screen stays open after a failure, so the player can try again without losing what was typed.
        private bool CallProfileService(Action<IPlayerProfileService> serviceCall)
        {
            IPlayerProfileService profileService = _playerProfileServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)profileService;
            bool isDone = false;
            try
            {
                serviceCall(profileService);
                isDone = true;
                communicationObject.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The server refused a change of the own profile with the fault {FaultCode}.", ex.Detail.Code);
                ShowRefusal(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not change the own profile.");
                RequestErrorDialog(ErrorDialogKind.ServiceUnavailable);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The profile service could not be reached to change the own profile.");
                RequestErrorDialog(ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The profile service timed out while changing the own profile.");
                RequestErrorDialog(ErrorDialogKind.ConnectionLost);
            }

            return isDone;
        }

        private void ShowRefusal(ServiceFaultCode code)
        {
            switch (code)
            {
                case ServiceFaultCode.SessionExpired:
                    RequestErrorDialog(ErrorDialogKind.SessionExpired);
                    break;
                case ServiceFaultCode.RequiredField:
                    ErrorMessageKey = RequiredFieldKey;
                    break;
                case ServiceFaultCode.DisallowedCharacters:
                    ErrorMessageKey = DisallowedCharactersKey;
                    break;
                case ServiceFaultCode.InvalidImage:
                    ErrorMessageKey = UnsupportedImageKey;
                    break;
                case ServiceFaultCode.InvalidUrl:
                    ErrorMessageKey = InvalidUrlKey;
                    break;
                default:
                    RequestErrorDialog(ErrorDialogKind.ServiceUnavailable);
                    break;
            }
        }

        private void RequestErrorDialog(ErrorDialogKind kind)
        {
            ErrorDialogRequested?.Invoke(this, kind);
        }
    }
}
