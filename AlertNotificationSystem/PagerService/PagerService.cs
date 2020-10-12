using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlertNotificationSystem.PagerService
{
    public class PagerService : IPagerService
    {
        private readonly IPersistenceAdapter _persistenceAdapter;
        private readonly IEscalationPolicyAdapter _escalationPolicyAdapter;
        private readonly ITimerAdapter _timerAdapter;
        private readonly IEmailAdapater _emailAdapter;
        private readonly ISmsAdapater _smsAdapter;

        private TimeSpan _acknoledgementTimeout = new TimeSpan(hours: 0, minutes: 15, seconds: 0);

        private Dictionary<int, HealthStatusEvent> _serviceHealthStatus = new Dictionary<int, HealthStatusEvent>();

        public PagerService(
            IPersistenceAdapter persistenceAdapter,
            IEscalationPolicyAdapter escalationPolicyAdapter,
            ITimerAdapter timerAdapter,
            IEmailAdapater emailAdapater,
            ISmsAdapater smsAdapater)
        {
            _persistenceAdapter = persistenceAdapter;
            _escalationPolicyAdapter = escalationPolicyAdapter;
            _timerAdapter = timerAdapter;
            _emailAdapter = emailAdapater;
            _smsAdapter = smsAdapater;
        }

        public void Acknowledgment(int monitoredServiceId)
        {
            var monitoredService = _persistenceAdapter.RetrieveMonitoredService(monitoredServiceId);
            if (!monitoredService.IsHealthy)
            {
                _persistenceAdapter.Acknowledgement(monitoredServiceId);
            }
        }

        public void AcknowledgmentTimeout(int monitoredServiceId)
        {
            var monitoredService = _persistenceAdapter.RetrieveMonitoredService(monitoredServiceId);

            if (monitoredService.IsHealthy) return;

            if (_serviceHealthStatus.ContainsKey(monitoredService.ServiceId) && _serviceHealthStatus[monitoredService.ServiceId].IsHealthy)
            {
                _persistenceAdapter.UpdateHealthStatus(monitoredService.Id, isHealthy: true);
                return;
            }

            var escalationPolicy = _escalationPolicyAdapter.RetrieveEscalationPolicy(monitoredService.ServiceId);
            var hasLastLevelBeenNotifed = monitoredService.EscalationLevel == escalationPolicy.Levels.Max(l => l.EscalationLevel);

            if (!monitoredService.IsAlertAcknowledged && !hasLastLevelBeenNotifed)
            {
                var nextExcalationLevel = monitoredService.EscalationLevel + 1;
                var nextLevelTargets = escalationPolicy.Levels.Single(l => l.EscalationLevel == nextExcalationLevel);
                NotifyTargets(nextLevelTargets.Targets);
                _timerAdapter.CreateTimer(monitoredService.Id, _acknoledgementTimeout);
                _persistenceAdapter.UpdateEscalationLevel(monitoredService.Id, nextExcalationLevel);
            }
        }

        public void Alert(AlertEvent alert)
        {
            var monitoredService = _persistenceAdapter.CreateOrRetrieveMonitoredService(alert.ServiceId);
            if (monitoredService.IsHealthy)
            {
                _persistenceAdapter.UpdateHealthStatus(monitoredService.Id, isHealthy: false);

                var escalationPolicy = _escalationPolicyAdapter.RetrieveEscalationPolicy(monitoredService.ServiceId);
                var firstLevelTargets = escalationPolicy.Levels.Single(l => l.EscalationLevel == 1);
                NotifyTargets(firstLevelTargets.Targets);

                _timerAdapter.CreateTimer(monitoredService.Id, _acknoledgementTimeout);
            }
        }

        public void UpdateHealthStatus(HealthStatusEvent healthStatusEvent)
        {
            _serviceHealthStatus[healthStatusEvent.ServiceId] = healthStatusEvent;
        }

        private void NotifyTargets(List<Target> targets)
        {
            foreach (var target in targets)
            {
                switch (target.Type)
                {
                    case TargetType.Email:
                        _emailAdapter.SendEmail(target.ContactInfo);
                        break;
                    case TargetType.Sms:
                        _smsAdapter.SendSms(target.ContactInfo);
                        break;
                }
            }
        }
    }
}
