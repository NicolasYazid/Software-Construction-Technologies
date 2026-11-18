using System;

using GinRummy.Contracts.Services.Friendship;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Notifiers;

namespace GinRummy.Services.Services.Friendship
{
    public class FriendshipServiceTools
    {
        public FriendshipServiceTools(AccountAccessPolicy accessPolicy, OnlinePlayers<IFriendshipCallback> onlinePlayers)
        {
            AccessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            OnlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public AccountAccessPolicy AccessPolicy { get; private set; }
        public OnlinePlayers<IFriendshipCallback> OnlinePlayers { get; private set; }
    }
}
