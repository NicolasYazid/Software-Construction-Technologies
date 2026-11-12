using System;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class Email
    {
        public const int DestinationAddressMaxLength = 254;

        public const int DeliveryStatusNameMaxLength = 20;

        public Email(int verificationCodeId, string destinationAddress, DateTime sendAt, EmailDeliveryStatus deliveryStatus)
        {
            ValidateVerificationCodeId(verificationCodeId);
            ValidateDestinationAddress(destinationAddress);
            ValidateDeliveryStatus(deliveryStatus);

            VerificationCodeId = verificationCodeId;
            DestinationAddress = destinationAddress.Trim();
            SendAt = sendAt;
            DeliveryStatusName = deliveryStatus.ToString();
        }

        protected Email()
        {
        }

        public int EmailId { get; private set; }

        public int VerificationCodeId { get; private set; }

        public string DestinationAddress { get; private set; }

        public DateTime SendAt { get; private set; }

        public string DeliveryStatusName { get; private set; }

        public EmailDeliveryStatus DeliveryStatus
        {
            get { return (EmailDeliveryStatus)Enum.Parse(typeof(EmailDeliveryStatus), DeliveryStatusName); }
        }

        private static void ValidateVerificationCodeId(int verificationCodeId)
        {
            if (verificationCodeId <= 0)
            {
                throw new ArgumentException("Verification code id must be positive.", nameof(verificationCodeId));
            }
        }

        private static void ValidateDestinationAddress(string destinationAddress)
        {
            if (string.IsNullOrWhiteSpace(destinationAddress))
            {
                throw new ArgumentException("Destination address is required.", nameof(destinationAddress));
            }

            if (destinationAddress.Trim().Length > DestinationAddressMaxLength)
            {
                throw new ArgumentException("Destination address is too long.", nameof(destinationAddress));
            }
        }

        private static void ValidateDeliveryStatus(EmailDeliveryStatus deliveryStatus)
        {
            if (!Enum.IsDefined(typeof(EmailDeliveryStatus), deliveryStatus))
            {
                throw new ArgumentOutOfRangeException(nameof(deliveryStatus), "Delivery status is not valid.");
            }
        }
    }
}
