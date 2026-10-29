using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// The email sent to deliver a verification code, mapped to the Email table. Each
    /// verification code has at most one email. Protects its own invariants: it always
    /// references a code and has a destination that fits the column.
    /// </summary>
    public class Email
    {
        /// <summary>
        /// Length of the destination_address column, the same as the account email.
        /// </summary>
        public const int DestinationAddressMaxLength = 254;

        /// <summary>
        /// Length of the delivery_status column.
        /// </summary>
        public const int DeliveryStatusNameMaxLength = 20;

        /// <summary>
        /// Creates a valid new email record.
        /// </summary>
        /// <param name="verificationCodeId">Identifier of the code the email delivers; must be positive.</param>
        /// <param name="destinationAddress">Address the email was sent to; required, at most 254 characters.</param>
        /// <param name="sendAt">Moment the email was handed to the mail service, in UTC.</param>
        /// <param name="deliveryStatus">Outcome of handing the email to the mail service.</param>
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

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Email()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int EmailId { get; private set; }

        /// <summary>
        /// Gets the identifier of the verification code the email delivers.
        /// </summary>
        public int VerificationCodeId { get; private set; }

        /// <summary>
        /// Gets the address the email was sent to.
        /// </summary>
        public string DestinationAddress { get; private set; }

        /// <summary>
        /// Gets the moment the email was handed to the mail service, in UTC.
        /// </summary>
        public DateTime SendAt { get; private set; }

        /// <summary>
        /// Gets the delivery status exactly as the delivery_status column stores it.
        /// </summary>
        public string DeliveryStatusName { get; private set; }

        /// <summary>
        /// Gets the outcome of handing the email to the mail service.
        /// </summary>
        public EmailDeliveryStatus DeliveryStatus
        {
            get { return (EmailDeliveryStatus)Enum.Parse(typeof(EmailDeliveryStatus), DeliveryStatusName); }
        }

        // An email always delivers a code that already exists.
        private static void ValidateVerificationCodeId(int verificationCodeId)
        {
            if (verificationCodeId <= 0)
            {
                throw new ArgumentException("Verification code id must be positive.", nameof(verificationCodeId));
            }
        }

        // The destination must fit the column, which has the same length as the account email.
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

        // The column only accepts the names of the enum, so a number outside it is rejected.
        private static void ValidateDeliveryStatus(EmailDeliveryStatus deliveryStatus)
        {
            if (!Enum.IsDefined(typeof(EmailDeliveryStatus), deliveryStatus))
            {
                throw new ArgumentOutOfRangeException(nameof(deliveryStatus), "Delivery status is not valid.");
            }
        }
    }
}
