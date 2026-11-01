using System;
using System.ServiceModel;

using GinRummy.Client.PlayerPresenceServiceReference;

namespace GinRummy.Client.Services
{
    /// <summary>
    /// Receives the presence pushes of the duplex service and re-raises them as a plain event, so
    /// the lobby view model does not implement the generated callback contract itself. It does not
    /// use the WCF synchronization context, so a push never blocks waiting for the UI thread; the
    /// view model marshals the change to the UI thread on its own.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false)]
    internal class PlayerPresenceCallbackHandler : IPlayerPresenceServiceCallback
    {
        /// <summary>
        /// Raised when the server pushes a friend's presence change.
        /// </summary>
        public event EventHandler<FriendPresence> FriendPresenceChanged;

        /// <summary>
        /// Called by WCF when one of the player's friends changes presence.
        /// </summary>
        /// <param name="presence">The friend and their new presence state.</param>
        public void OnFriendPresenceChanged(FriendPresence presence)
        {
            FriendPresenceChanged?.Invoke(this, presence);
        }
    }
}
