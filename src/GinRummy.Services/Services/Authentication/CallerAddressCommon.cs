using System.ServiceModel;
using System.ServiceModel.Channels;

namespace GinRummy.Services.Services.Authentication
{
    public static class CallerAddressCommon
    {
        private const string UnknownCaller = "unknown";

        public static string DescribeCaller()
        {
            string caller = UnknownCaller;
            OperationContext context = OperationContext.Current;
            if (context != null)
            {
                RemoteEndpointMessageProperty remoteEndpoint =
                    context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (remoteEndpoint != null)
                {
                    caller = remoteEndpoint.Address + ":" + remoteEndpoint.Port;
                }
            }

            return caller;
        }
    }
}
