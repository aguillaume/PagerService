using System.Collections.Generic;

namespace AlertNotificationSystem.PagerService
{
    public class Level
    {
        public int EscalationLevel { get; set; }
        public List<Target> Targets { get; set; }
    }
}