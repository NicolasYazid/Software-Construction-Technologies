using System;
using System.Collections.Generic;
using System.ServiceModel;

using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Hosting
{
    public class ServiceHostGroup
    {
        private readonly List<ServiceHost> _hosts;
        private readonly ILogger<ServiceHostGroup> _hostLogger;

        public ServiceHostGroup(ILogger<ServiceHostGroup> hostLogger)
        {
            _hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
            _hosts = new List<ServiceHost>();
        }

        public void Add(ServiceHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            host.Faulted += OnHostFaulted;
            _hosts.Add(host);
        }

        public void OpenAll()
        {
            int openedHosts = 0;
            foreach (ServiceHost host in _hosts)
            {
                if (TryOpen(host))
                {
                    openedHosts++;
                }
            }

            _hostLogger.LogInformation("{OpenedHosts} of {TotalHosts} services are listening.", openedHosts, _hosts.Count);
        }

        public void CloseAll()
        {
            foreach (ServiceHost host in _hosts)
            {
                CloseHost(host);
            }
        }

        private static string GetServiceName(ServiceHostBase host)
        {
            return host.Description.Name;
        }

        private bool TryOpen(ServiceHost host)
        {
            bool isOpen = false;
            try
            {
                host.Open();
                isOpen = true;
                _hostLogger.LogInformation("Service {ServiceName} is listening.", GetServiceName(host));
            }
            catch (CommunicationException ex)
            {
                _hostLogger.LogCritical(ex, "Service {ServiceName} could not open its endpoint.", GetServiceName(host));
                host.Abort();
            }
            catch (TimeoutException ex)
            {
                _hostLogger.LogCritical(ex, "Service {ServiceName} took too long to open.", GetServiceName(host));
                host.Abort();
            }

            return isOpen;
        }

        private void CloseHost(ServiceHost host)
        {
            try
            {
                if (host.State == CommunicationState.Opened)
                {
                    host.Close();
                }
                else
                {
                    host.Abort();
                }
            }
            catch (CommunicationException ex)
            {
                _hostLogger.LogWarning(ex, "Service {ServiceName} did not close cleanly.", GetServiceName(host));
                host.Abort();
            }
            catch (TimeoutException ex)
            {
                _hostLogger.LogWarning(ex, "Service {ServiceName} took too long to close.", GetServiceName(host));
                host.Abort();
            }
        }

        private void OnHostFaulted(object sender, EventArgs e)
        {
            if (sender is ServiceHostBase host)
            {
                _hostLogger.LogError("Service {ServiceName} faulted and stopped listening; the other services keep running.",
                    GetServiceName(host));
            }
        }
    }
}
