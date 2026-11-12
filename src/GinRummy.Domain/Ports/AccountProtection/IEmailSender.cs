using GinRummy.Domain.Entities.AccountProtection;

namespace GinRummy.Domain.Ports.AccountProtection
{
    public interface IEmailSender
    {
        EmailDeliveryStatus Send(string destinationAddress, string subject, string body);
    }
}
