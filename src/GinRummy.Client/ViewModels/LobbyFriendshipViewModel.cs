using System;
using System.Collections.Generic;
using System.Windows.Input;

using GinRummy.Client.FriendshipServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class LobbyFriendshipViewModel : ViewModelBase
    {
        private const string ServiceUnavailableKey = "Error_SysServiceUnavailable";
        private const string PlayerNotFoundKey = "Error_SocialPlayerNotFound";
        private const string RequestToYourselfKey = "LobbyChat_FriendRequestToYourself";
        private const string AlreadyFriendsKey = "LobbyChat_FriendRequestAlreadyFriends";
        private const string RequestAlreadySentKey = "LobbyChat_FriendRequestAlreadySent";
        private const string IncomingRequestExistsKey = "LobbyChat_IncomingRequestExists";
        private const string TooManyPendingRequestsKey = "LobbyChat_FriendRequestTooManyPending";
        private const string OwnFriendLimitKey = "LobbyChat_FriendRequestOwnFriendLimit";
        private const string PlayerFriendLimitKey = "LobbyChat_FriendRequestPlayerFriendLimit";
        private const string TooManyRequestsPerMinuteKey = "LobbyChat_FriendRequestTooManyPerMinute";
        private const string WaitingTimeKey = "LobbyChat_FriendRequestWaitingTime";

        private static readonly Dictionary<FriendRequestOutcome, string> _noticeKeysByOutcome =
            new Dictionary<FriendRequestOutcome, string>
            {
                { FriendRequestOutcome.PlayerNotFound, PlayerNotFoundKey },
                { FriendRequestOutcome.SelfRequest, RequestToYourselfKey },
                { FriendRequestOutcome.AlreadyFriends, AlreadyFriendsKey },
                { FriendRequestOutcome.RequestAlreadySent, RequestAlreadySentKey },
                { FriendRequestOutcome.IncomingRequestExists, IncomingRequestExistsKey },
                { FriendRequestOutcome.AddresseeHasTooManyRequests, TooManyPendingRequestsKey },
                { FriendRequestOutcome.SenderFriendLimitReached, OwnFriendLimitKey },
                { FriendRequestOutcome.AddresseeFriendLimitReached, PlayerFriendLimitKey },
                { FriendRequestOutcome.TooManyRequestsPerMinute, TooManyRequestsPerMinuteKey },
                { FriendRequestOutcome.WaitingTimeNotOver, WaitingTimeKey }
            };

        private readonly FriendshipConnection _friendshipConnection;
        private readonly LogInOutcome _signedInPlayer;
        private string _noticeKey;

        public LobbyFriendshipViewModel(FriendshipConnection friendshipConnection, LogInOutcome signedInPlayer)
        {
            _friendshipConnection = friendshipConnection ?? throw new ArgumentNullException(nameof(friendshipConnection));
            _signedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            DismissNoticeCommand = new RelayCommand(() => NoticeKey = null);
        }

        public ICommand DismissNoticeCommand { get; private set; }

        public string NoticeKey
        {
            get { return _noticeKey; }
            private set
            {
                _noticeKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasNotice));
            }
        }

        public bool HasNotice
        {
            get { return _noticeKey != null; }
        }

        public void Start()
        {
            if (!_friendshipConnection.Start(_signedInPlayer.AccessToken))
            {
                NoticeKey = ServiceUnavailableKey;
            }
        }

        public void Stop()
        {
            _friendshipConnection.Stop();
        }

        public void SendFriendRequest(LobbyPlayerViewModel player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (!player.PublicTag.HasValue)
            {
                return;
            }

            FriendRequestOutcome? outcome = _friendshipConnection.SendFriendRequest(_signedInPlayer.AccessToken, player.PublicTag.Value);
            string noticeKey = ServiceUnavailableKey;
            if (outcome.HasValue)
            {
                noticeKey = FindNoticeKey(outcome.Value);
                MarkPendingRequest(player, outcome.Value);
            }

            NoticeKey = noticeKey;
        }

        private static string FindNoticeKey(FriendRequestOutcome outcome)
        {
            string noticeKey;
            _noticeKeysByOutcome.TryGetValue(outcome, out noticeKey);

            return noticeKey;
        }

        private static void MarkPendingRequest(LobbyPlayerViewModel player, FriendRequestOutcome outcome)
        {
            if ((outcome == FriendRequestOutcome.Sent) || (outcome == FriendRequestOutcome.RequestAlreadySent))
            {
                player.HasPendingRequest = true;
            }
        }
    }
}
