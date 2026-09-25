using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Settings of the account the profile panel shows (P14). The password never travels to
    /// the client, so it has no field here.
    /// </summary>
    public sealed class AccountSettingsDto
    {
        /// <summary>
        /// Gets or sets the email address of the account.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets whether the sign-in asks for a code besides the password.
        /// </summary>
        public bool IsTwoStepEnabled { get; set; }
        /// <summary>
        /// Gets or sets the master volume, from zero to one hundred.
        /// </summary>
        public double MasterVolume { get; set; }
        /// <summary>
        /// Gets or sets whether the chat shows the censored mark instead of the words the
        /// filter catches.
        /// </summary>
        public bool IsChatFilterEnabled { get; set; }
        /// <summary>
        /// Gets or sets the external accounts the player can link, linked or not.
        /// </summary>
        public IList<LinkedAccountDto> LinkedAccounts { get; set; }
    }
}
