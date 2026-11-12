using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace GinRummy.Services.Hosting
{
    public class ServiceInstanceProvider : IInstanceProvider
    {
        private readonly Func<object> _createInstance;

        public ServiceInstanceProvider(Func<object> createInstance)
        {
            _createInstance = createInstance ?? throw new ArgumentNullException(nameof(createInstance));
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return _createInstance();
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return _createInstance();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instance is IDisposable disposableInstance)
            {
                disposableInstance.Dispose();
            }
        }
    }
}
