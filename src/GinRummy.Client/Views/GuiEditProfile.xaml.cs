using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using GinRummy.Client.Models;
using GinRummy.Client.Services;
using Microsoft.Win32;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Edit profile screen (P15). Serves CU-28, CU-29, CU-30, CU-31 and CU-32: the picture,
    /// the name, the biography and the social links of the player.
    /// </summary>
    public partial class GuiEditProfile : GuiWindowBase
    {
        private const int MaxBioLength = 500;
        private const long MaxImageBytes = 2L * 1024 * 1024;
        private const string MaxImageSizeText = "2 MB";
        private const string ImageFilter = "*.jpg;*.jpeg;*.png|*.jpg;*.jpeg;*.png";
        private const string BioCounterKey = "EditProfile_LblBioCounter";
        private const string ImageRequirementsKey = "EditProfile_LblImageRequirements";
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string UnsupportedImageKey = "Error_ValUnsupportedImage";

        private readonly ObservableCollection<SocialLinkDto> _socialLinks;

        /// <summary>
        /// Builds the screen with the current profile of the player.
        /// </summary>
        public GuiEditProfile()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            PlayerProfileDto profile = dataService.GetOwnProfile();
            _socialLinks = new ObservableCollection<SocialLinkDto>(profile.SocialLinks);
            lstSocialLinks.ItemsSource = _socialLinks;
            cmbPlatform.ItemsSource = dataService.GetPlatforms();
            cmbPlatform.SelectedIndex = 0;
            txtUsername.Text = profile.Username;
            txtBio.MaxLength = MaxBioLength;
            txtBio.Text = profile.Bio;
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the counter of the biography and the requirements of the picture, which
        /// carry a placeholder.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lblBioCounter != null)
            {
                lblBioCounter.Text = Localization.Format(BioCounterKey, MaxBioLength - txtBio.Text.Length);
                lblImageRequirements.Text = Localization.Format(ImageRequirementsKey, MaxImageSizeText);
            }
        }

        private void OnBioTextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFormattedText();
        }

        private void OnChangePhotoClick(object sender, RoutedEventArgs e)
        {
            // The filter shows only the patterns and no description, because the description of
            // a filter is visible text that the dictionary does not carry.
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ImageFilter;
            if (fileDialog.ShowDialog(this) == true)
            {
                ShowPicture(fileDialog.FileName);
            }
        }

        private void OnRemovePhotoClick(object sender, RoutedEventArgs e)
        {
            // Without a picture the profile shows the default avatar again (CU-28 FA-03).
            imgProfilePicture.Source = null;
        }

        private void OnAddSocialLinkClick(object sender, RoutedEventArgs e)
        {
            lblPlatform.Visibility = Visibility.Visible;
            btnAddSocialLink.Visibility = Visibility.Collapsed;
            txtSocialUrl.Focus();
        }

        private void OnUnlinkClick(object sender, RoutedEventArgs e)
        {
            SocialLinkDto link = ((FrameworkElement)sender).DataContext as SocialLinkDto;
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.SocialLinkRemove);
            confirmDialog.Owner = this;
            confirmDialog.ShowDialog();
            if (confirmDialog.IsConfirmed)
            {
                _socialLinks.Remove(link);
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            lblUsernameChanged.Visibility = Visibility.Collapsed;
            bool isLinkPending = lblPlatform.Visibility == Visibility.Visible;
            bool isComplete = txtUsername.Text.Trim().Length > 0
                && (!isLinkPending || txtSocialUrl.Text.Trim().Length > 0);
            if (isComplete)
            {
                // The server validates the name, the biography and the address before keeping
                // them (CU-29, CU-30, CU-31); the screen shows the result once it answers.
                if (isLinkPending)
                {
                    SaveSocialLink();
                }

                HideError();
                lblUsernameChanged.Visibility = Visibility.Visible;
            }
            else
            {
                ShowError(RequiredFieldKey);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveSocialLink()
        {
            string platformName = cmbPlatform.SelectedItem as string;
            SocialLinkDto existing = FindSocialLink(platformName);
            bool isSaved = true;

            // A profile keeps one link per platform, so a second one replaces the first after
            // asking (CU-31 FA-03).
            if (existing != null)
            {
                GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.SocialLinkReplace);
                confirmDialog.Owner = this;
                confirmDialog.ShowDialog();
                isSaved = confirmDialog.IsConfirmed;
                if (isSaved)
                {
                    _socialLinks.Remove(existing);
                }
            }

            if (isSaved)
            {
                SocialLinkDto link = new SocialLinkDto();
                link.PlatformName = platformName;
                link.Url = txtSocialUrl.Text.Trim();
                _socialLinks.Add(link);
                txtSocialUrl.Clear();
                lblPlatform.Visibility = Visibility.Collapsed;
                btnAddSocialLink.Visibility = Visibility.Visible;
            }
        }

        private SocialLinkDto FindSocialLink(string platformName)
        {
            SocialLinkDto found = null;
            foreach (SocialLinkDto link in _socialLinks)
            {
                if (string.Equals(link.PlatformName, platformName, StringComparison.Ordinal))
                {
                    found = link;
                }
            }

            return found;
        }

        private void ShowPicture(string path)
        {
            FileInfo file = new FileInfo(path);
            BitmapImage picture = null;
            if (file.Length <= MaxImageBytes)
            {
                picture = LoadPicture(path);
            }

            // A file too large or that cannot be decoded as an image keeps the current picture
            // (CU-28 EX-01).
            if (picture == null)
            {
                ShowError(UnsupportedImageKey);
            }
            else
            {
                HideError();
                imgProfilePicture.Source = picture;
            }
        }

        private static BitmapImage LoadPicture(string path)
        {
            BitmapImage picture = new BitmapImage();
            try
            {
                picture.BeginInit();
                picture.CacheOption = BitmapCacheOption.OnLoad;
                picture.UriSource = new Uri(path);
                picture.EndInit();
            }
            catch (NotSupportedException)
            {
                picture = null;
            }
            catch (FileFormatException)
            {
                picture = null;
            }

            return picture;
        }

        private void ShowError(string messageKey)
        {
            lblErrorMessage.Text = Localization.GetText(messageKey);
            lblErrorMessage.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            lblErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
}
