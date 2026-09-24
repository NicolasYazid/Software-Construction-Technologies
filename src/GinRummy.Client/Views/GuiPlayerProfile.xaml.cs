using System.Globalization;
using System.Windows;

using GinRummy.Client.Models;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Profile screen of a player (P21). Implements CU-27 for the profile of another player,
    /// for the own one (FA-02) and for the reduced one of a guest (FA-04), and starts CU-12,
    /// CU-16 and CU-18 from its actions.
    /// </summary>
    public partial class GuiPlayerProfile : GuiWindowBase
    {
        private const string CountFormat = "N0";
        private const string RateFormat = "P0";

        private readonly PlayerProfileDto _profile;

        /// <summary>
        /// Builds the screen for one profile.
        /// </summary>
        /// <param name="profile">Profile to show, with its relation to the player.</param>
        public GuiPlayerProfile(PlayerProfileDto profile)
        {
            InitializeComponent();
            _profile = profile;
            DataContext = profile;
            Title = profile.Username;
            ApplyRelation();
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the numbers of the performance, whose separators and percent sign depend
        /// on the culture (CU-20 step 6).
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (_profile != null)
            {
                CultureInfo culture = Localization.CurrentCulture;
                lblMatchesPlayedValue.Text = _profile.MatchesPlayed.ToString(CountFormat, culture);
                lblWinRateValue.Text = _profile.WinRate.ToString(RateFormat, culture);
                lblScoreValue.Text = _profile.Score.ToString(CountFormat, culture);
            }
        }

        private void ApplyRelation()
        {
            bool isPlayer = !_profile.IsGuest;
            bool isOther = isPlayer && !_profile.IsOwnProfile;
            bool hasMatches = _profile.MatchesPlayed > 0;

            // A guest has no profile of its own: only its temporary name and its presence are
            // shown (CU-27 FA-04). A profile without links leaves out its section (FA-03).
            lblGuestHasNoProfile.Visibility = VisibilityCommon.FromCondition(_profile.IsGuest);
            lblRankName.Visibility = VisibilityCommon.FromCondition(isPlayer);
            lblPublicTag.Visibility = VisibilityCommon.FromCondition(isPlayer);
            lblBio.Visibility = VisibilityCommon.FromCondition(isPlayer);
            lblSocialLinks.Visibility = VisibilityCommon.FromCondition(isPlayer && _profile.SocialLinks.Count > 0);
            lblMatchesPlayed.Visibility = VisibilityCommon.FromCondition(isPlayer && hasMatches);
            lblNoMatchesYet.Visibility = VisibilityCommon.FromCondition(isPlayer && !hasMatches);
            btnReport.Visibility = VisibilityCommon.FromCondition(isOther);
            btnAddFriend.Visibility = VisibilityCommon.FromCondition(isOther && !_profile.IsFriend);
            btnRemoveFriend.Visibility = VisibilityCommon.FromCondition(isOther && _profile.IsFriend);
            btnEditProfile.Visibility = VisibilityCommon.FromCondition(_profile.IsOwnProfile);
        }

        private void OnReportClick(object sender, RoutedEventArgs e)
        {
            GuiReportPlayer reportPlayer = new GuiReportPlayer(_profile.Username);
            reportPlayer.Owner = this;
            reportPlayer.ShowDialog();
        }

        private void OnAddFriendClick(object sender, RoutedEventArgs e)
        {
            // The request is created by the server (CU-12 FA-01); the screen has nothing to
            // change until it answers.
        }

        private void OnRemoveFriendClick(object sender, RoutedEventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.RemoveFriend);
            confirmDialog.Owner = this;
            confirmDialog.ShowDialog();
        }

        private void OnEditProfileClick(object sender, RoutedEventArgs e)
        {
            GuiEditProfile editProfile = new GuiEditProfile();
            editProfile.Owner = this;
            editProfile.ShowDialog();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
