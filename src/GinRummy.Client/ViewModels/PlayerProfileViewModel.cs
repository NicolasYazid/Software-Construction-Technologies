using System;
using System.Collections.Generic;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class PlayerProfileViewModel : ViewModelBase
    {
        private readonly PlayerProfile _profile;

        public PlayerProfileViewModel(PlayerProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            AddFriendCommand = new RelayCommand(AddFriend);
            RemoveFriendCommand = new RelayCommand(RemoveFriend);
        }

        public ICommand AddFriendCommand { get; private set; }
        public ICommand RemoveFriendCommand { get; private set; }

        public string Username
        {
            get { return _profile.Username; }
        }

        public string PublicTag
        {
            get { return _profile.PublicTag; }
        }

        public string RankName
        {
            get { return _profile.RankName; }
        }

        public string Bio
        {
            get { return _profile.Bio; }
        }

        public IList<SocialLink> SocialLinks
        {
            get { return _profile.SocialLinks; }
        }

        public int MatchesPlayed
        {
            get { return _profile.MatchesPlayed; }
        }

        public double WinRate
        {
            get { return _profile.WinRate; }
        }

        public int Score
        {
            get { return _profile.Score; }
        }

        public bool IsGuest
        {
            get { return _profile.IsGuest; }
        }

        // A guest has no profile of its own, so only its temporary name is shown (CU-27 FA-04).
        public bool IsPlayer
        {
            get { return !_profile.IsGuest; }
        }

        public bool IsOwnProfile
        {
            get { return _profile.IsOwnProfile; }
        }

        // A profile without links leaves out its section (CU-27 FA-03).
        public bool HasSocialLinks
        {
            get { return IsPlayer && (_profile.SocialLinks.Count > 0); }
        }

        public bool HasMatches
        {
            get { return IsPlayer && (_profile.MatchesPlayed > 0); }
        }

        public bool HasNoMatchesYet
        {
            get { return IsPlayer && (_profile.MatchesPlayed == 0); }
        }

        public bool CanAddFriend
        {
            get { return IsOtherPlayer && !_profile.IsFriend; }
        }

        public bool CanSendFriendRequest
        {
            get { return !_profile.HasPendingRequest; }
        }

        public bool CanRemoveFriend
        {
            get { return IsOtherPlayer && _profile.IsFriend; }
        }

        private bool IsOtherPlayer
        {
            get { return IsPlayer && !_profile.IsOwnProfile; }
        }

        // Once the server creates the request, the action stays disabled while it waits for an answer.
        // This keeps a second request from being sent (CU-12 and its EX-07).
        private void AddFriend()
        {
            _profile.HasPendingRequest = true;
            RaisePropertyChanged(nameof(CanSendFriendRequest));
        }

        // Without the friendship, the profile offers a request again (CU-16 Post-3).
        private void RemoveFriend()
        {
            _profile.IsFriend = false;
            RaisePropertyChanged(nameof(CanAddFriend));
            RaisePropertyChanged(nameof(CanRemoveFriend));
        }
    }
}
