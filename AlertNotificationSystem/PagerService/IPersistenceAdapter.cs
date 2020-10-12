using System;
using System.Collections.Generic;
using System.Text;

namespace AlertNotificationSystem.PagerService
{
    public interface IPersistenceAdapter
    {
        /// <summary>
        /// Allows to update the health status for a monitored service
        /// </summary>
        /// <param name="monitoredServiceId">The monitored service to update</param>
        /// <param name="isHealthy">The status of the monitored service</param>
        void UpdateHealthStatus(int monitoredServiceId, bool isHealthy);

        /// <summary>
        /// Created a new monitored service record or retrieve an existing one for a given service that is at fault
        /// </summary>
        /// <param name="serviceId">The service id at fault</param>
        /// <returns>The state of the monitored service just created</returns>
        MonitoredService CreateOrRetrieveMonitoredService(int serviceId);

        /// <summary>
        /// Retrieve the monitored service information
        /// </summary>
        /// <param name="monitoredServiceId">The monitor service id to be retrieved</param>
        /// <returns>The current state of the monitored service</returns>
        MonitoredService RetrieveMonitoredService(int monitoredServiceId);

        /// <summary>
        /// Allows the Escalation level for a monitored service to be updated
        /// </summary>
        /// <param name="monitoredServiceId">The monitored service id to be updated</param>
        /// <param name="excalationLevel">The escalation level to be saved</param>
        void UpdateEscalationLevel(int monitoredServiceId, int excalationLevel);

        /// <summary>
        /// Allows the acknowledgment flag to be updated for a monitored service
        /// </summary>
        /// <param name="monitoredServiceId">The monitored service id to be updated</param>
        void Acknowledgement(int monitoredServiceId);
    }
}
