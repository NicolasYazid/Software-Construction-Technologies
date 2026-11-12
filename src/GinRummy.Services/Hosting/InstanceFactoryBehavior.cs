using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace GinRummy.Services.Hosting
{
    public class InstanceFactoryBehavior : IServiceBehavior
    {
        private readonly Func<object> _createInstance;

        public InstanceFactoryBehavior(Func<object> createInstance)
        {
            _createInstance = createInstance ?? throw new ArgumentNullException(nameof(createInstance));
        }

        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

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

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

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
