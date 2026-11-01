using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Infrastructure;

namespace GinRummy.Infrastructure
{
    /// <summary>
    /// Fulfils IEmailSender over SMTP with System.Net.Mail. It is the adapter the use cases reach through the
    /// IEmailSender port to deliver verification codes, so none of them depends on the mail service behind it.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;

        /// <summary>
        /// Builds the sender with the mail service values the composition root read from the configuration.
        /// </summary>
        /// <param name="settings">Values of the mail service; may be incomplete.</param>
        /// <exception cref="ArgumentNullException">Thrown when no settings are given.</exception>
        public SmtpEmailSender(SmtpSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Sends a plain text email.
        /// </summary>
        /// <param name="destinationAddress">Address that receives the message.</param>
        /// <param name="subject">Subject line, already translated.</param>
        /// <param name="body">Message text, already translated.</param>
        /// <returns>Sent when the mail service accepted the message; Failed otherwise.</returns>
        public EmailDeliveryStatus Send(string destinationAddress, string subject, string body)
        {
            EmailDeliveryStatus deliveryStatus = EmailDeliveryStatus.Failed;
            if (_settings.IsComplete)
            {
                deliveryStatus = Deliver(destinationAddress, subject, body);
            }

            return deliveryStatus;
        }

        // A delivery that does not go through is an expected outcome, not an error: the use case keeps the
        // verification code and records its Email as Failed so the player can ask for the code again
        // (CU-01 EX-06, CU-08 EX-07). That is why each exception becomes the Failed status instead of propagating.
        private EmailDeliveryStatus Deliver(string destinationAddress, string subject, string body)
        {
            EmailDeliveryStatus deliveryStatus;
            try
            {
                MailAddress senderAddress = new MailAddress(_settings.SenderAddress, _settings.SenderName, Encoding.UTF8);
                MailAddress recipientAddress = new MailAddress(destinationAddress);
                using (MailMessage message = new MailMessage(senderAddress, recipientAddress))
                {
                    // The encoding is set explicitly because the Spanish texts carry accents and the letter ñ.
                    message.Subject = subject;
                    message.SubjectEncoding = Encoding.UTF8;
                    message.Body = body;
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = false;
                    bool isDelivered = DeliverWithinTimeout(message);
                    deliveryStatus = isDelivered ? EmailDeliveryStatus.Sent : EmailDeliveryStatus.Failed;
                }
            }
            catch (AggregateException)
            {
                deliveryStatus = EmailDeliveryStatus.Failed;
            }
            catch (SmtpException)
            {
                deliveryStatus = EmailDeliveryStatus.Failed;
            }
            catch (FormatException)
            {
                deliveryStatus = EmailDeliveryStatus.Failed;
            }
            catch (ArgumentException)
            {
                deliveryStatus = EmailDeliveryStatus.Failed;
            }

            return deliveryStatus;
        }

        // SmtpClient.Timeout does not cover opening the connection: against a server that never answers, Windows gives
        // up only after about 21 seconds. The wait bounds the whole delivery instead, and leaving the using block
        // aborts a delivery that is still running, so a mail service that does not answer ends as Failed (QAS-12).
        private bool DeliverWithinTimeout(MailMessage message)
        {
            bool isDelivered;
            using (SmtpClient smtpClient = BuildClient())
            {
                Task delivery = smtpClient.SendMailAsync(message);
                isDelivered = delivery.Wait(_settings.TimeoutMilliseconds);
            }

            return isDelivered;
        }

        private SmtpClient BuildClient()
        {
            SmtpClient smtpClient = new SmtpClient(_settings.Host, _settings.Port);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            // The sender's password and the codes must never travel in plain text. SmtpClient only supports
            // STARTTLS, which is why the configured port must be 587 and not the implicit TLS port 465.
            smtpClient.EnableSsl = true;

            // Setting UseDefaultCredentials clears Credentials, so it has to be set first.
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_settings.SenderAddress, _settings.SenderPassword);

            // The default of 100 seconds is longer than the 30 second sendTimeout of the server binding.
            smtpClient.Timeout = _settings.TimeoutMilliseconds;

            return smtpClient;
        }
    }
}