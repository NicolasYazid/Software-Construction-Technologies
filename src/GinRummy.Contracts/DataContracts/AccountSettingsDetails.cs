using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// What the account settings show of the own account (CU-04 to CU-07).
    /// </summary>
    [DataContract]
    public class AccountSettingsDetails
    {
        /// <summary>
        /// Email of the account as the server stored it.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Whether the account asks for a code emailed at each sign-in.
        /// </summary>
        [DataMember]
        public bool IsTwoFactorEnabled { get; set; }

        /// <summary>
        /// Whether the player reads the messages with prohibited words as censored.
        /// </summary>
        [DataMember]
        public bool IsChatFilterEnabled { get; set; }
    }
}
