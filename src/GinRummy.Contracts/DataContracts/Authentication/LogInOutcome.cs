using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Authentication
{
    [DataContract]
    public class LogInOutcome
    {
        [DataMember]
        public bool IsTwoFactorRequired { get; set; }

        [DataMember]
        public string AccessToken { get; set; }

        [DataMember]
        public DateTime? AccessTokenExpiresAt { get; set; }

        [DataMember]
        public int PlayerAccountId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int PublicTag { get; set; }

        [DataMember]
        public string LocaleCode { get; set; }

        [DataMember]
        public bool IsChatFilterEnabled { get; set; }
    }
}
