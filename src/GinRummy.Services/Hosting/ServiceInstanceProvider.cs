using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace GinRummy.Services.Hosting
{
    /// <summary>
    /// Creates every service instance with the factory the composition root supplies. WCF can
    /// then build a new instance per call while the service still receives its use cases
    /// through the constructor.
    /// </summary>
    public class ServiceInstanceProvider : IInstanceProvider
    {
        private readonly Func<object> _createInstance;

        /// <summary>
        /// Builds the provider with the factory of the service.
        /// </summary>
        /// <param name="createInstance">Function that composes a new service instance.</param>
        public ServiceInstanceProvider(Func<object> createInstance)
        {
            _createInstance = createInstance ?? throw new ArgumentNullException(nameof(createInstance));
        }

        /// <summary>
        /// Creates a new service instance.
        /// </summary>
        /// <param name="instanceContext">Context of the new instance.</param>
        /// <returns>The new service instance.</returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            return _createInstance();
        }

        /// <summary>
        /// Creates a new service instance for an incoming message.
        /// </summary>
        /// <param name="instanceContext">Context of the new instance.</param>
        /// <param name="message">Message that triggered the creation.</param>
        /// <returns>The new service instance.</returns>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return _createInstance();
        }

        /// <summary>
        /// Releases the instance when WCF recycles it, so a service that holds resources frees
        /// them at the end of each call.
        /// </summary>
        /// <param name="instanceContext">Context of the instance.</param>
        /// <param name="instance">Instance being recycled.</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instance is IDisposable disposableInstance)
            {
                disposableInstance.Dispose();
            }
        }
    }
}
