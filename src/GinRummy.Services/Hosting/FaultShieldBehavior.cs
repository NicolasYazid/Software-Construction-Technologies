using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace GinRummy.Services.Hosting
{
    /// <summary>
    /// Adds the shared error handler to every channel of a service host, so no service sends
    /// internal details to the client and every unexpected error reaches the log.
    /// </summary>
    public class FaultShieldBehavior : IServiceBehavior
    {
        private readonly IErrorHandler _errorHandler;

        /// <summary>
        /// Builds the behavior with the handler to install.
        /// </summary>
        /// <param name="errorHandler">Handler that logs errors and shields their details.</param>
        public FaultShieldBehavior(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        /// <summary>
        /// Adds nothing to the bindings. The signature is fixed by IServiceBehavior.
        /// </summary>
        /// <param name="serviceDescription">Description of the service.</param>
        /// <param name="serviceHostBase">Host of the service.</param>
        /// <param name="endpoints">Endpoints of the service.</param>
        /// <param name="bindingParameters">Parameters of the bindings.</param>
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Installs the error handler on every channel of the host.
        /// </summary>
        /// <param name="serviceDescription">Description of the service.</param>
        /// <param name="serviceHostBase">Host of the service.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase dispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                if (dispatcherBase is ChannelDispatcher channelDispatcher)
                {
                    channelDispatcher.ErrorHandlers.Add(_errorHandler);
                }
            }
        }

        /// <summary>
        /// Validates nothing; the behavior only adds a handler.
        /// </summary>
        /// <param name="serviceDescription">Description of the service.</param>
        /// <param name="serviceHostBase">Host of the service.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
