using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Enums;

namespace GinRummy.Services.Mappers
{
    /// <summary>
    /// Turns a friend's domain presence into the presence row the client shows.
    /// </summary>
    public static class FriendPresenceMapper
    {
        /// <summary>
        /// Builds the presence row of one friend from their public tag, name and domain state.
        /// </summary>
        /// <param name="publicTag">Public tag of the friend.</param>
        /// <param name="username">Display name of the friend.</param>
        /// <param name="state">Domain presence state of the friend.</param>
        /// <returns>The presence row the client shows.</returns>
        public static FriendPresence ToFriendPresence(int publicTag, string username, PresenceState state)
        {
            FriendPresence presence = new FriendPresence
            {
                PublicTag = publicTag,
                Username = username,
                Status = ToPresenceStatus(state)
            };

            return presence;
        }

        /// <summary>
        /// Maps a domain presence state to the presence status of the contract.
        /// </summary>
        /// <param name="state">Domain presence state.</param>
        /// <returns>The matching contract status.</returns>
        public static PresenceStatus ToPresenceStatus(PresenceState state)
        {
            PresenceStatus status = PresenceStatus.Offline;
            switch (state)
            {
                case PresenceState.Online:
                    status = PresenceStatus.Online;
                    break;
                case PresenceState.Playing:
                    status = PresenceStatus.Playing;
                    break;
                default:
                    status = PresenceStatus.Offline;
                    break;
            }

            return status;
        }
    }
}
