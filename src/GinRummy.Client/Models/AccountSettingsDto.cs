using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    // Settings of the account the profile panel shows (P14). The password never travels to the
    // client, so it has no field here.
    public sealed class AccountSettingsDto
    {
        public string Email { get; set; }
        public bool IsTwoStepEnabled { get; set; }
        public double MasterVolume { get; set; }
        // When enabled, the chat shows the censored mark instead of the words the filter
        // catches.
        public bool IsChatFilterEnabled { get; set; }
        public IList<LinkedAccountDto> LinkedAccounts { get; set; }
    }
}
