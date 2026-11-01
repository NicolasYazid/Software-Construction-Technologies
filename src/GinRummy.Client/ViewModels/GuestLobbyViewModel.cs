using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.PlayerPresenceServiceReference;
using GinRummy.Client.Services;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class GuestLobbyViewModel : ViewModelBase
    {
        private readonly LobbySnapshot _lobby;
        private readonly int? _presencePublicTag;
        private readonly Func<InstanceContext, IPlayerPresenceService> _presenceServiceFactory;
        private readonly ILogger<GuestLobbyViewModel> _logger;
        private readonly SynchronizationContext _uiContext;

        private PlayerPresenceCallbackHandler _presenceCallback;
        private IPlayerPresenceService _presenceService;
        private bool _isSearchingMatch;

        public GuestLobbyViewModel(SampleDataService dataService)
            : this(dataService, null, null, null)
        {
        }

        // presencePublicTag and the factory are null for a plain guest lobby; when they are provided the
        // friends tab is driven live by the duplex presence service instead of showing the disabled notice.
        public GuestLobbyViewModel(
            SampleDataService dataService,
            int? presencePublicTag,
            Func<InstanceContext, IPlayerPresenceService> presenceServiceFactory,
            ILogger<GuestLobbyViewModel> logger)
        {
            if (dataService == null)
            {
                throw new ArgumentNullException(nameof(dataService));
            }

            _lobby = dataService.GetGuestLobby();
            _presencePublicTag = presencePublicTag;
            _presenceServiceFactory = presenceServiceFactory;
            _logger = logger;
            _uiContext = SynchronizationContext.Current;
            FriendsOnline = new ObservableCollection<LobbyPlayerViewModel>();
            FriendsUnavailable = new ObservableCollection<LobbyPlayerViewModel>();
            FindMatchCommand = new RelayCommand(FindMatch);
            CancelSearchCommand = new RelayCommand(CancelSearch);
        }

        public ICommand FindMatchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }

        // Live friends of the presence flow, split into the two groups the friends tab shows.
        public ObservableCollection<LobbyPlayerViewModel> FriendsOnline { get; private set; }
        public ObservableCollection<LobbyPlayerViewModel> FriendsUnavailable { get; private set; }

        // True once a player identity drives the friends tab, so the view shows the live list; false for a guest.
        public bool IsPresenceEnabled
        {
            get { return _presencePublicTag.HasValue && (_presenceServiceFactory != null); }
        }

        public bool IsFriendsListUnavailable
        {
            get { return !IsPresenceEnabled; }
        }

        public string PlayerName
        {
            get { return _lobby.PlayerName; }
        }

        public IList<ChatEntry> ChatEntries
        {
            get { return _lobby.ChatEntries; }
        }

        public IList<LobbyPlayerViewModel> Online
        {
            get { return _lobby.Online; }
        }

        public IList<MatchPair> MatchesInProgress
        {
            get { return _lobby.MatchesInProgress; }
        }

        public int PlayersInMatchCount
        {
            get { return _lobby.PlayersInMatchCount; }
        }

        public IList<LobbyPlayerViewModel> Unavailable
        {
            get { return _lobby.Unavailable; }
        }

        public bool IsSearchingMatch
        {
            get { return _isSearchingMatch; }
            private set
            {
                _isSearchingMatch = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanFindMatch));
            }
        }

        public bool CanFindMatch
        {
            get { return !_isSearchingMatch; }
        }

        /// <summary>
        /// Subscribes to the presence of the player's friends and fills the two friend groups with
        /// their current state. Does nothing for a plain guest lobby.
        /// </summary>
        public void StartPresence()
        {
            if (IsPresenceEnabled)
            {
                _presenceCallback = new PlayerPresenceCallbackHandler();
                _presenceCallback.FriendPresenceChanged += OnFriendPresenceChanged;
                if (TryCreatePresenceChannel())
                {
                    LoadFriendsPresence(_presencePublicTag.Value);
                }
            }
        }

        // Building the duplex channel reads the client configuration, which can throw if the endpoint
        // is misconfigured. Any failure here must not crash the lobby, so it is caught and logged.
        private bool TryCreatePresenceChannel()
        {
            bool isCreated = false;
            try
            {
                _presenceService = _presenceServiceFactory(new InstanceContext(_presenceCallback));
                isCreated = true;
            }
            catch (InvalidOperationException ex)
            {
                _presenceService = null;
                LogPresenceFailure(ex, "The presence endpoint is misconfigured; the friends tab stays empty.");
            }

            return isCreated;
        }

        /// <summary>
        /// Unsubscribes from presence and closes the duplex channel when the lobby closes.
        /// </summary>
        public void StopPresence()
        {
            if (_presenceService != null)
            {
                ClosePresenceChannel(_presenceService);
                _presenceService = null;
            }

            if (_presenceCallback != null)
            {
                _presenceCallback.FriendPresenceChanged -= OnFriendPresenceChanged;
                _presenceCallback = null;
            }
        }

        private void LoadFriendsPresence(int subscriberPublicTag)
        {
            ICommunicationObject communicationObject = (ICommunicationObject)_presenceService;
            try
            {
                FriendPresence[] friendsPresence = _presenceService.SubscribeToFriendsPresence(subscriberPublicTag);
                FillFriendGroups(friendsPresence);
                LogPresenceInfo("Presence subscription succeeded with " + friendsPresence.Length + " friend(s).");
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _presenceService = null;
                LogPresenceFailure(ex, "The server refused the presence subscription.");
            }
            catch (EndpointNotFoundException ex)
            {
                communicationObject.Abort();
                _presenceService = null;
                LogPresenceFailure(ex, "The presence service endpoint was not found.");
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _presenceService = null;
                LogPresenceFailure(ex, "The presence service could not be reached to subscribe.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _presenceService = null;
                LogPresenceFailure(ex, "The presence service timed out while subscribing.");
            }
            catch (InvalidOperationException ex)
            {
                communicationObject.Abort();
                _presenceService = null;
                LogPresenceFailure(ex, "The presence channel could not be opened to subscribe.");
            }
        }

        private void FillFriendGroups(FriendPresence[] friendsPresence)
        {
            FriendsOnline.Clear();
            FriendsUnavailable.Clear();
            foreach (FriendPresence friendPresence in friendsPresence)
            {
                PlaceFriend(BuildFriendRow(friendPresence));
            }
        }

        // The callback runs off the UI thread, so the change is posted to the UI context before touching the collections.
        private void OnFriendPresenceChanged(object sender, FriendPresence presence)
        {
            if (_uiContext != null)
            {
                _uiContext.Post(state => ApplyFriendPresence((FriendPresence)state), presence);
            }
            else
            {
                ApplyFriendPresence(presence);
            }
        }

        private void ApplyFriendPresence(FriendPresence presence)
        {
            RemoveFriend(presence.PublicTag);
            PlaceFriend(BuildFriendRow(presence));
        }

        private static LobbyPlayerViewModel BuildFriendRow(FriendPresence presence)
        {
            LobbyPlayerViewModel friendRow = new LobbyPlayerViewModel
            {
                Username = presence.Username,
                PublicTag = presence.PublicTag,
                IsFriend = true,
                IsOnline = presence.Status == PresenceStatus.Online
            };

            return friendRow;
        }

        private void PlaceFriend(LobbyPlayerViewModel friendRow)
        {
            if (friendRow.IsOnline)
            {
                FriendsOnline.Add(friendRow);
            }
            else
            {
                FriendsUnavailable.Add(friendRow);
            }
        }

        private void RemoveFriend(int publicTag)
        {
            RemoveFriendFrom(FriendsOnline, publicTag);
            RemoveFriendFrom(FriendsUnavailable, publicTag);
        }

        private static void RemoveFriendFrom(ObservableCollection<LobbyPlayerViewModel> friends, int publicTag)
        {
            LobbyPlayerViewModel existingFriend = null;
            foreach (LobbyPlayerViewModel friend in friends)
            {
                if (friend.PublicTag == publicTag)
                {
                    existingFriend = friend;
                }
            }

            if (existingFriend != null)
            {
                friends.Remove(existingFriend);
            }
        }

        private void ClosePresenceChannel(IPlayerPresenceService presenceService)
        {
            ICommunicationObject communicationObject = (ICommunicationObject)presenceService;
            try
            {
                presenceService.UnsubscribeFromFriendsPresence(_presencePublicTag.Value);
                communicationObject.Close();
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                LogPresenceFailure(ex, "The presence channel could not be closed cleanly.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                LogPresenceFailure(ex, "The presence channel timed out while closing.");
            }
        }

        private void LogPresenceFailure(Exception ex, string message)
        {
            if (_logger != null)
            {
                _logger.LogError(ex, message);
            }
        }

        private void LogPresenceInfo(string message)
        {
            if (_logger != null)
            {
                _logger.LogInformation(message);
            }
        }

        // The search stays in the state of CU-22 FA-03, the one the prototype draws, until the server pairs the players.
        private void FindMatch()
        {
            IsSearchingMatch = true;
        }

        private void CancelSearch()
        {
            IsSearchingMatch = false;
        }
    }
}
