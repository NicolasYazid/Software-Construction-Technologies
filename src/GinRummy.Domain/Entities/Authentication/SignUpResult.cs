using System;

using GinRummy.Domain.Entities.AccountProtection;

namespace GinRummy.Domain.Entities.Authentication
{
    public class SignUpResult
    {
        public SignUpResult(PlayerAccount account, EmailDeliveryStatus deliveryStatus)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            DeliveryStatus = deliveryStatus;
        }

        public PlayerAccount Account { get; private set; }

        public EmailDeliveryStatus DeliveryStatus { get; private set; }

        public bool IsVerificationCodeSent
        {
            get { return DeliveryStatus == EmailDeliveryStatus.Sent; }
        }
    }
}
