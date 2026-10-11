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
        PasswordRecovery = 2
    }
}
