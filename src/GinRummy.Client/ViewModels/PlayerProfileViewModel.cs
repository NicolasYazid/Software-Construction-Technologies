using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.PlayerProfileServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class PlayerProfileViewModel : ViewModelBase
    {
        private readonly PlayerProfile _profile;
        private readonly Func<IPlayerProfileService> _playerProfileServiceFactory;
        private readonly ILogger<PlayerProfileViewModel> _logger;

        // A client is created for every load because a client whose channel failed cannot be opened again.
        public PlayerProfileViewModel(
            PlayerProfile profile,
            Func<IPlayerProfileService> playerProfileServiceFactory,
            ILogger<PlayerProfileViewModel> logger)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _playerProfileServiceFactory = playerProfileServiceFactory
                ?? throw new ArgumentNullException(nameof(playerProfileServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            AddFriendCommand = new RelayCommand(AddFriend);
            RemoveFriendCommand = new RelayCommand(RemoveFriend);
        }

        public event EventHandler<ErrorDialogKind> ProfileUnavailable;

        public ICommand AddFriendCommand { get; private set; }
        public ICommand RemoveFriendCommand { get; private set; }

        public string Username
        {
            get { return _profile.Username; }
        }

        public int PublicTag
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

        public bool IsPlayer
        {
            get { return !_profile.IsGuest; }
        }

        public bool IsOwnProfile
        {
            get { return _profile.IsOwnProfile; }
        }

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

        // A guest has no profile on the server, so only the profile of a registered player is read from it (CU-27 FA-04).
        public void LoadProfile(string accessToken)
        {
            if (IsPlayer)
            {
                ReadProfileFromServer(accessToken);
            }
        }

        // Another player wrote the address, so only a web address is opened and nothing else starts on this computer.
        public void OpenSocialLink(SocialLink link)
        {
            Uri address;
            bool isAbsolute = Uri.TryCreate(link.Url, UriKind.Absolute, out address);
            bool isWebAddress = isAbsolute
                && ((address.Scheme == Uri.UriSchemeHttp) || (address.Scheme == Uri.UriSchemeHttps));
            if (!isWebAddress)
            {
                _logger.LogWarning("The social link of {PublicTag} was not opened because it is not a web address.", _profile.PublicTag);
            }
            else
            {
                try
                {
                    Process.Start(address.AbsoluteUri);
                }
                catch (Win32Exception ex)
                {
                    _logger.LogWarning(ex, "No program on this computer could open the social link {Url}.", address.AbsoluteUri);
                }
            }
        }

        // The editor changes the same profile this screen shows, so only the screen needs to be told.
        public void RefreshProfile()
        {
            RaisePropertyChanged(string.Empty);
        }

        // Once the server creates the request, the action stays disabled while it waits for an answer.
        // This keeps a second request from being sent (CU-12 and its EX-07).
        private void AddFriend()
        {
            _profile.HasPendingRequest = true;
            RaisePropertyChanged(nameof(CanSendFriendRequest));
        }

        private void RemoveFriend()
        {
            _profile.IsFriend = false;
            RaisePropertyChanged(nameof(CanAddFriend));
            RaisePropertyChanged(nameof(CanRemoveFriend));
        }

        private void ReadProfileFromServer(string accessToken)
        {
            IPlayerProfileService playerProfileService = _playerProfileServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)playerProfileService;
            try
            {
                ShowDetails(playerProfileService.GetPlayerProfile(accessToken, _profile.PublicTag));
                communicationObject.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(
                    ex,
                    "The server refused the profile of player {PublicTag} with the fault {FaultCode}.",
                    _profile.PublicTag,
                    ex.Detail.Code);
                NotifyRefusal(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not read the profile of player {PublicTag}.", _profile.PublicTag);
                NotifyProfileUnavailable(ErrorDialogKind.ServiceUnavailable);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The profile service could not be reached to read a profile.");
                NotifyProfileUnavailable(ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The profile service timed out while reading a profile.");
                NotifyProfileUnavailable(ErrorDialogKind.ConnectionLost);
            }
        }

        private void ShowDetails(PlayerProfileDetails details)
        {
            _profile.Username = details.Username;
            _profile.RankName = details.RankName;
            _profile.Bio = details.Bio;
            _profile.SocialLinks = details.SocialLinks
                .Select(link => new SocialLink(link.SocialMediaId, link.PlatformName, link.Url))
                .ToList();
            _profile.MatchesPlayed = details.MatchesPlayed;
            _profile.WinRate = details.WinRate;
            _profile.Score = details.Score;
            RaisePropertyChanged(string.Empty);
        }

        private void NotifyRefusal(ServiceFaultCode code)
        {
            ErrorDialogKind kind = ErrorDialogKind.ServiceUnavailable;
            if (code == ServiceFaultCode.SessionExpired)
            {
                kind = ErrorDialogKind.SessionExpired;
            }
            else if (code == ServiceFaultCode.PlayerNotFound)
            {
                kind = ErrorDialogKind.DataNotFound;
                _profile.IsRemoved = true;
            }

            NotifyProfileUnavailable(kind);
        }

        private void NotifyProfileUnavailable(ErrorDialogKind kind)
        {
            ProfileUnavailable?.Invoke(this, kind);
        }
    }
}
