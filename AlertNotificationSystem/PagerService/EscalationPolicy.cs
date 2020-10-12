using System.Collections.Generic;

namespace AlertNotificationSystem.PagerService
{
    public class EscalationPolicy
    {
        public int MonitoredServiceId { get; set; }
        public List<Level> Levels { get; set; }
    }
}