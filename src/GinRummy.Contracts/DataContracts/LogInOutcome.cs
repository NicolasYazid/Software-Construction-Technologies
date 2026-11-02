using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Answer to a sign-in whose credentials were accepted: the player who signed in and the access token the
    /// client presents in every later request, unless the code of the second factor is still pending.
    /// </summary>
    [DataContract]
    public class LogInOutcome
    {
        /// <summary>
        /// Whether the code of the second factor was emailed and the token waits for it. When true, the
        /// token and player members are empty.
        /// </summary>
        [DataMember]
        public bool IsTwoFactorRequired { get; set; }

        /// <summary>
        /// Signed access token that identifies the player; empty while the second factor is pending.
        /// </summary>
        [DataMember]
        public string AccessToken { get; set; }

        /// <summary>
        /// Moment the access token stops being valid, in UTC; null while the second factor is pending.
        /// </summary>
        [DataMember]
        public DateTime? AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// Identifier of the player who signed in; zero while the second factor is pending.
        /// </summary>
        [DataMember]
        public int PlayerId { get; set; }

        /// <summary>
        /// Display name of the player who signed in; empty while the second factor is pending.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Public tag shown to other players; zero while the second factor is pending.
        /// </summary>
        [DataMember]
        public int PublicTag { get; set; }
    }
}
