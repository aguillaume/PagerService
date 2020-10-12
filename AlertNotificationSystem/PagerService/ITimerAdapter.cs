using System;
using System.Collections.Generic;
using System.Text;

namespace AlertNotificationSystem.PagerService
{
    public interface ITimerAdapter
    {
        /// <summary>
        /// Creates a new timer for a monitored service and a given amount of time
        /// </summary>
        /// <param name="monitoredServiceId">The monitored service id associated with the timer</param>
        /// <param name="delay">The amount of time the timer should run for</param>
        void CreateTimer(int monitoredServiceId, TimeSpan delay);
    }
}
