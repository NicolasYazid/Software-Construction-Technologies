using System;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class EmailDelivery
    {
        public EmailDelivery(DateTime sendAt, EmailDeliveryStatus deliveryStatus)
        {
            SendAt = sendAt;
            DeliveryStatus = deliveryStatus;
        }

        public DateTime SendAt { get; private set; }

        public EmailDeliveryStatus DeliveryStatus { get; private set; }
    }
}
