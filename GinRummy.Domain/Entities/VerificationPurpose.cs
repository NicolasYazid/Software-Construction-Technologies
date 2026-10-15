using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// Reason a verification code was generated. The numbers match the rows already
    /// seeded in the Purpose table.
    /// </summary>
    public enum VerificationPurpose
    {
        /// <summary>
        /// The code confirms the email address of a new account.
        /// </summary>
        CreateAccount = 1,
        /// <summary>
        /// The code allows the player to set a new password.
        /// </summary>
        PasswordRecovery = 2,
        /// <summary>
        /// The code confirms that the player activates two-step verification.
        /// </summary>
        TwoFactorSetup = 3,
        /// <summary>
        /// The code is the second factor requested when a player signs in.
        /// </summary>
        TwoFactorLogin = 4,
        /// <summary>
        /// The code confirms a password change made from inside the account.
        /// </summary>
        PasswordChange = 5,
        /// <summary>
        /// The code confirms that the player owns the new email address of the account.
        /// </summary>
        EmailChange = 6
    }
}
