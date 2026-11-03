using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Entities.Authentication
{
    /// <summary>
    /// Outcome of a created account: the account and whether the mail service accepted the email with its
    /// code. The account exists in both cases, because a failed email can be sent again (CU-01 Post-5).
    /// </summary>
    public class SignUpResult
    {
        /// <summary>
        /// Builds the outcome of a created account.
        /// </summary>
        /// <param name="account">Account that was created, with its player loaded.</param>
        /// <param name="deliveryStatus">Status the mail service reported for the email with the code.</param>
        /// <exception cref="ArgumentNullException">Thrown when the account is null.</exception>
        public SignUpResult(PlayerAccount account, EmailDeliveryStatus deliveryStatus)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            DeliveryStatus = deliveryStatus;
        }

        /// <summary>
        /// Gets the account that was created, with its player loaded.
        /// </summary>
        public PlayerAccount Account { get; private set; }

        /// <summary>
        /// Gets the status the mail service reported for the email with the code.
        /// </summary>
        public EmailDeliveryStatus DeliveryStatus { get; private set; }

        /// <summary>
        /// Gets whether the mail service accepted the email with the code.
        /// </summary>
        public bool IsVerificationCodeSent
        {
            get { return DeliveryStatus == EmailDeliveryStatus.Sent; }
        }
    }
}
