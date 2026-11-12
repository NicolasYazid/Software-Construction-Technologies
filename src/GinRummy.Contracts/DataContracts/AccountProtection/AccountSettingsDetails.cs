using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.AccountProtection
{
    [DataContract]
    public class AccountSettingsDetails
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public bool IsTwoFactorEnabled { get; set; }

        [DataMember]
        public bool IsChatFilterEnabled { get; set; }
    }
}
