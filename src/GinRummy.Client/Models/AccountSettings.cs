using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    // The password never travels to the client, so it has no field here.
    public sealed class AccountSettings
    {
        public string Email { get; set; }
        public bool IsTwoStepEnabled { get; set; }
        public double MasterVolume { get; set; }
        public bool IsChatFilterEnabled { get; set; }
        public IList<LinkedAccount> LinkedAccounts { get; set; }
    }
}
