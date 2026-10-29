using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class EditProfileViewModel : ViewModelBase
    {
        private const int DefaultPlatformIndex = 0;
        private const string MaxImageSizeLabel = "2 MB";

        private string _bio;
        private string _socialUrl;
        private string _picturePath;
        private bool _isNewLinkFormOpen;
        private bool _isRequiredFieldShown;
        private bool _isUsernameChangedShown;
        private SocialLink _linkToRemove;
        private SocialLink _linkToReplace;

        public EditProfileViewModel(SampleDataService dataService)
        {
            if (dataService == null)
            {
                throw new ArgumentNullException(nameof(dataService));
            }

            PlayerProfile profile = dataService.GetOwnProfile();
            SocialLinks = new ObservableCollection<SocialLink>(profile.SocialLinks);
            Platforms = dataService.GetPlatforms();
            SelectedPlatform = Platforms[DefaultPlatformIndex];
            Username = profile.Username;
            _bio = profile.Bio ?? string.Empty;
            _socialUrl = string.Empty;
            ChangePictureCommand = new RelayCommand(ChangePicture);
            RemovePictureCommand = new RelayCommand(RemovePicture);
            AddSocialLinkCommand = new RelayCommand(OpenNewLinkForm);
            UnlinkCommand = new RelayCommand(RequestSocialLinkRemoval);
            SaveCommand = new RelayCommand(Save);
        }

        public event EventHandler SocialLinkRemovalRequested;

        public event EventHandler SocialLinkReplaceRequested;

        public ObservableCollection<SocialLink> SocialLinks { get; private set; }
        public IList<string> Platforms { get; private set; }
        public string SelectedPlatform { get; set; }
        public string Username { get; set; }
        public ICommand ChangePictureCommand { get; private set; }
        public ICommand RemovePictureCommand { get; private set; }
        public ICommand AddSocialLinkCommand { get; private set; }
        public ICommand UnlinkCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

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

        public string SocialUrl
        {
            get { return _socialUrl; }
            set
            {
                _socialUrl = value ?? string.Empty;
                RaisePropertyChanged();
            }
        }

        public string PicturePath
        {
            get { return _picturePath; }
            private set
            {
                _picturePath = value;
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

        public bool IsRequiredFieldShown
        {
            get { return _isRequiredFieldShown; }
            private set
            {
                _isRequiredFieldShown = value;
                RaisePropertyChanged();
            }
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

        public void CompleteSocialLinkRemoval(bool isConfirmed)
        {
            if (isConfirmed)
            {
                SocialLinks.Remove(_linkToRemove);
            }

            _linkToRemove = null;
        }

        // A profile keeps one link per platform, so a second link replaces the first after confirmation (CU-31 FA-03).
        public void CompleteSocialLinkReplace(bool isConfirmed)
        {
            if (isConfirmed)
            {
                SocialLinks.Remove(_linkToReplace);
                AddSocialLink(_linkToReplace.PlatformName);
            }

            _linkToReplace = null;
        }

        // The picture shows as soon as it is chosen, while the CU-28 service of the server, which validates and stores it, is pending.
        private void ChangePicture(object parameter)
        {
            PicturePath = parameter as string;
        }

        // Without a picture the profile shows the default avatar again (CU-28 FA-03).
        private void RemovePicture()
        {
            PicturePath = null;
        }

        private void OpenNewLinkForm()
        {
            IsNewLinkFormOpen = true;
        }

        private void RequestSocialLinkRemoval(object parameter)
        {
            _linkToRemove = parameter as SocialLink;
            SocialLinkRemovalRequested?.Invoke(this, EventArgs.Empty);
        }

        // Only the required fields are checked here because the server validates the rest (CU-29, CU-30, CU-31).
        private void Save()
        {
            IsUsernameChangedShown = false;
            bool isLinkPending = _isNewLinkFormOpen;
            bool hasUsername = !string.IsNullOrWhiteSpace(Username);
            bool hasSocialUrl = _socialUrl.Trim().Length > 0;
            bool isComplete = hasUsername && (!isLinkPending || hasSocialUrl);
            IsRequiredFieldShown = !isComplete;
            if (isComplete)
            {
                if (isLinkPending)
                {
                    SaveSocialLink();
                }

                IsUsernameChangedShown = true;
            }
        }

        private void SaveSocialLink()
        {
            SocialLink existing = FindSocialLink(SelectedPlatform);
            if (existing == null)
            {
                AddSocialLink(SelectedPlatform);
            }
            else
            {
                _linkToReplace = existing;
                SocialLinkReplaceRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void AddSocialLink(string platformName)
        {
            SocialLink link = new SocialLink();
            link.PlatformName = platformName;
            link.Url = _socialUrl.Trim();
            SocialLinks.Add(link);
            SocialUrl = string.Empty;
            IsNewLinkFormOpen = false;
        }

        private SocialLink FindSocialLink(string platformName)
        {
            SocialLink found = null;
            foreach (SocialLink link in SocialLinks)
            {
                if (string.Equals(link.PlatformName, platformName, StringComparison.Ordinal))
                {
                    found = link;
                }
            }

            return found;
        }
    }
}
