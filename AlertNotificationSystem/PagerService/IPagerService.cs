using System;
using System.Collections.Generic;
using System.Text;

namespace AlertNotificationSystem.PagerService
{
    public interface IPagerService
    {
        /// <summary>
        /// Notify the pager service that the is a new alert for to be monitored.
        /// </summary>
        /// <param name="alert">The alert event for the service</param>
        void Alert(AlertEvent alert);

        /// <summary>
        /// Notify the pager service when the acknowledgment timeout to check if the next escalation level should be notified
        /// </summary>
        /// <param name="monitoredServiceId">The monitored service associated to the timeout</param>
        void AcknowledgmentTimeout(int monitoredServiceId);

        /// <summary>
        /// Notify the pager service that a notified target has acknowledged the alert
        /// </summary>
        /// <param name="monitoredServiceId">The monitored service associated to the acknowledgment </param>
        void Acknowledgment(int monitoredServiceId);

        /// <summary>
        /// NNotify the pager service a service is not healthy
        /// </summary>
        /// <param name="healthStatusEvent">The service health status information</param>
        void UpdateHealthStatus(HealthStatusEvent healthStatusEvent);
    }
}
