namespace GinRummy.Domain.Entities.Authentication
{
    /// <summary>
    /// Outcome of handing an email to the mail service. The names match the values the
    /// delivery_status column of the Email table accepts.
    /// </summary>
    public enum EmailDeliveryStatus
    {
        /// <summary>
        /// The mail service accepted the email.
        /// </summary>
        Sent,
        /// <summary>
        /// The mail service could not deliver the email.
        /// </summary>
        Failed
    }
}
