using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Answer to a sign-in whose credentials were accepted: the player who signed in and, unless the code of
    /// the second factor is still pending, the session that was opened.
    /// </summary>
    [DataContract]
    public class LogInOutcome
    {
        /// <summary>
        /// Whether the code of the second factor was emailed and the session waits for it. When true, the
        /// session and player members are empty.
        /// </summary>
        [DataMember]
        public bool IsTwoFactorRequired { get; set; }

        /// <summary>
        /// Identifier of the opened session; zero while the second factor is pending.
        /// </summary>
        [DataMember]
        public int PlayerSessionId { get; set; }

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
