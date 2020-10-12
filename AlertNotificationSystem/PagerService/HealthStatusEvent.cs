using System;
using System.Collections.Generic;
using System.Text;

namespace AlertNotificationSystem.PagerService
{
    public class HealthStatusEvent
    {
        public int ServiceId { get; set; }
        public bool IsHealthy { get; set; }
    }
}
