using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Authentication
{
    [DataContract]
    public class SignUpOutcome
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public bool IsVerificationCodeSent { get; set; }
    }
}
