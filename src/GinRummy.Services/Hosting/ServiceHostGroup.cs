using System;
using System.Collections.Generic;
using System.ServiceModel;

using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Hosting
{
    /// <summary>
    /// Opens and closes the server's services, one host per service. Each host is handled on its
    /// own, so a service that cannot open or that faults does not stop the others.
    /// </summary>
    public class ServiceHostGroup
    {
        private readonly List<ServiceHost> _hosts;
        private readonly ILogger<ServiceHostGroup> _hostLogger;

        /// <summary>
        /// Builds an empty group.
        /// </summary>
        /// <param name="hostLogger">Logger of the server.</param>
        public ServiceHostGroup(ILogger<ServiceHostGroup> hostLogger)
        {
            _hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
            _hosts = new List<ServiceHost>();
        }

        /// <summary>
        /// Adds a host that has not been opened yet.
        /// </summary>
        /// <param name="host">Host of one service.</param>
        public void Add(ServiceHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            host.Faulted += ReportFaultedHost;
            _hosts.Add(host);
        }

        /// <summary>
        /// Opens every host. A host that fails to open is logged and skipped.
        /// </summary>
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

        /// <summary>
        /// Closes every host. A host that cannot close cleanly is aborted.
        /// </summary>
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

        // A host that never opened or already faulted cannot close, so it is aborted to release its port.
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

        private void ReportFaultedHost(object sender, EventArgs eventArgs)
        {
            if (sender is ServiceHostBase host)
            {
                _hostLogger.LogError("Service {ServiceName} faulted and stopped listening; the other services keep running.",
                    GetServiceName(host));
            }
        }
    }
}
