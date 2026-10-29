using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace GinRummy.Services.Hosting
{
    /// <summary>
    /// Makes one service host build its instances with a given factory instead of a parameterless
    /// constructor. It is what lets a service run as PerCall while its dependencies are composed
    /// in one place.
    /// </summary>
    public class InstanceFactoryBehavior : IServiceBehavior
    {
        private readonly Func<object> _createInstance;

        /// <summary>
        /// Builds the behavior with the factory of the service.
        /// </summary>
        /// <param name="createInstance">Function that composes a new service instance.</param>
        public InstanceFactoryBehavior(Func<object> createInstance)
        {
            _createInstance = createInstance ?? throw new ArgumentNullException(nameof(createInstance));
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
        /// Installs the instance provider on every application endpoint of the host.
        /// </summary>
        /// <param name="serviceDescription">Description of the service.</param>
        /// <param name="serviceHostBase">Host of the service.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase dispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                if (dispatcherBase is ChannelDispatcher channelDispatcher)
                {
                    InstallProvider(channelDispatcher);
                }
            }
        }

        /// <summary>
        /// Validates nothing; the host already validates the service type.
        /// </summary>
        /// <param name="serviceDescription">Description of the service.</param>
        /// <param name="serviceHostBase">Host of the service.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        // System endpoints, such as metadata, keep their own provider because they do not create service instances.
        private void InstallProvider(ChannelDispatcher channelDispatcher)
        {
            foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
            {
                if (!endpointDispatcher.IsSystemEndpoint)
                {
                    endpointDispatcher.DispatchRuntime.InstanceProvider = new ServiceInstanceProvider(_createInstance);
                }
            }
        }
    }
}
