using System;
using System.Collections.Generic;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class GuestLobbyViewModel : ViewModelBase
    {
        private readonly LobbySnapshot _lobby;
        private bool _isSearchingMatch;

        public GuestLobbyViewModel(SampleDataService dataService)
        {
            if (dataService == null)
            {
                throw new ArgumentNullException(nameof(dataService));
            }

            _lobby = dataService.GetGuestLobby();
            FindMatchCommand = new RelayCommand(FindMatch);
            CancelSearchCommand = new RelayCommand(CancelSearch);
        }

        public ICommand FindMatchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }

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
