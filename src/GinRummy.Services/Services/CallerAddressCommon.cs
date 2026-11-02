using System.ServiceModel;
using System.ServiceModel.Channels;

namespace GinRummy.Services.Services
{
    /// <summary>
    /// Tells the address a request came from, for the log of the attempts the use cases ask to record with
    /// their origin (CU-01 EX-03, CU-02 EX-07).
    /// </summary>
    public static class CallerAddressCommon
    {
        private const string UnknownCaller = "unknown";

        /// <summary>
        /// Describes the address and port of the client that sent the current request.
        /// </summary>
        /// <returns>The address and port, or "unknown" outside a request.</returns>
        public static string DescribeCaller()
        {
            string caller = UnknownCaller;
            OperationContext context = OperationContext.Current;
            if (context != null)
            {
                // The address is read from the message itself, so the log does not depend on anything the client claims about itself.
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
