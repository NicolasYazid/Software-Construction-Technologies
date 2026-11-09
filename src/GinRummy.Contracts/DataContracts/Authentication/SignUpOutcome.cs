using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Authentication
{
    /// <summary>
    /// Answer to a created account. The account exists whether or not its code was emailed, because a failed
    /// email can be sent again from the verification screen (CU-01 EX-06).
    /// </summary>
    [DataContract]
    public class SignUpOutcome
    {
        /// <summary>
        /// Email of the new account as the server stored it; the client verifies and resends with this one.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Whether the mail service accepted the email with the verification code.
        /// </summary>
        [DataMember]
        public bool IsVerificationCodeSent { get; set; }
    }
}
