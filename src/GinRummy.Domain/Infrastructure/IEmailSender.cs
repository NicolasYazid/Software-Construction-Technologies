using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Infrastructure
{
    /// <summary>
    /// Contract the game's logic uses to send an email, without knowing which mail service is
    /// behind it. The outcome is reported instead of thrown, because a failed delivery does
    /// not cancel the operation that requested it: the verification code is kept and the
    /// failure is recorded.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends a plain text email.
        /// </summary>
        /// <param name="destinationAddress">Address that receives the message.</param>
        /// <param name="subject">Subject line, already translated.</param>
        /// <param name="body">Message text, already translated.</param>
        /// <returns>Sent when the mail service accepted the message; Failed otherwise.</returns>
        EmailDeliveryStatus Send(string destinationAddress, string subject, string body);
    }
}
