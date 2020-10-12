namespace AlertNotificationSystem.PagerService
{
    public class MonitoredService
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public bool IsHealthy { get; set; }
        public int EscalationLevel { get; set; }
        public bool IsAlertAcknowledged { get; set; }
    }
}