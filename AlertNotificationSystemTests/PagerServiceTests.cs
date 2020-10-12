using AlertNotificationSystem.PagerService;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlertNotificationSystemTests
{
    [TestFixture]
    public class PagerServiceTests
    {
        private Mock<IPersistenceAdapter> _persistenceAdapterMock;
        private Mock<IEscalationPolicyAdapter> _escalationPolicyAdapterMock;
        private Mock<IEmailAdapater> _emailAdapaterMock;
        private Mock<ISmsAdapater> _smsAdapaterMock;
        private Mock<ITimerAdapter> _timerAdapterMock;

        private const int MONITORED_SERVICE_ID = 123;
        private const int SERVICE_ID = 234;
        private const string ESCALATION_LEVEL_EMAIL_1 = "help-me-1@emai.com";
        private const string ESCALATION_LEVEL_EMAIL_2 = "help-me-2@emai.com";
        private const string ESCALATION_LEVEL_EMAIL_3 = "help-me-3@emai.com";
        private const string ESCALATION_LEVEL_SMS_1 = "0000001";
        private const string ESCALATION_LEVEL_SMS_2 = "0000002";
        private const int ESCALATION_LEVEL_1 = 1;
        private const int ESCALATION_LEVEL_2 = 2;
        private const int ESCALATION_LEVEL_3 = 3;

        private TimeSpan _timer15min = new TimeSpan(hours: 0, minutes: 15, seconds: 0);

        [SetUp]
        public void SetUp()
        {
            // MockBehavior.Strict ensures that if method is called without being configured test will fail
            _persistenceAdapterMock = new Mock<IPersistenceAdapter>(MockBehavior.Strict);
            _escalationPolicyAdapterMock = new Mock<IEscalationPolicyAdapter>(MockBehavior.Strict);
            _emailAdapaterMock = new Mock<IEmailAdapater>(MockBehavior.Strict);
            _smsAdapaterMock = new Mock<ISmsAdapater>(MockBehavior.Strict);
            _timerAdapterMock = new Mock<ITimerAdapter>(MockBehavior.Strict);
        }

        [Test]
        [Description("Checks that the correct targets are notified and a timer is set when there is a new alert")]
        public void NewAlertForMonitoredService()
        {
            // Set Up
            _persistenceAdapterMock.Setup(m => m.CreateOrRetrieveMonitoredService(SERVICE_ID)).Returns(GetMonitoredService());
            _persistenceAdapterMock.Setup(m => m.UpdateHealthStatus(MONITORED_SERVICE_ID, false));

            _escalationPolicyAdapterMock
                .Setup(m => m.RetrieveEscalationPolicy(SERVICE_ID))
                .Returns(GetEscalationPolicy());

            _emailAdapaterMock.Setup(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_1));

            _timerAdapterMock.Setup(m => m.CreateTimer(MONITORED_SERVICE_ID, _timer15min));
            
            // Act
            var pager = new PagerService(_persistenceAdapterMock.Object, _escalationPolicyAdapterMock.Object, _timerAdapterMock.Object,
                _emailAdapaterMock.Object, _smsAdapaterMock.Object) as IPagerService;
            pager.Alert(GetAlert());

            // Assert

            _persistenceAdapterMock.Verify(m => m.UpdateHealthStatus(MONITORED_SERVICE_ID, false), Times.Once);
            _persistenceAdapterMock.Verify(m => m.CreateOrRetrieveMonitoredService(SERVICE_ID), Times.Once);
            _escalationPolicyAdapterMock.Verify(m => m.RetrieveEscalationPolicy(SERVICE_ID), Times.Once);
            _emailAdapaterMock.Verify(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_1), Times.Once);
            _timerAdapterMock.Verify(m => m.CreateTimer(MONITORED_SERVICE_ID, _timer15min), Times.Once);
        }

        [Test]
        [Description("Checks that the next level of targets is notified on a acknowledgment timeout")]
        public void FollowUpNotificationOnAcknowledgementTimeout()
        {
            // Set up
            _persistenceAdapterMock.Setup(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID))
                .Returns(GetMonitoredService(isHealthy: false, escalationLevel: ESCALATION_LEVEL_2));
            _persistenceAdapterMock.Setup(m => m.UpdateEscalationLevel(MONITORED_SERVICE_ID, ESCALATION_LEVEL_3));

            _escalationPolicyAdapterMock
               .Setup(m => m.RetrieveEscalationPolicy(SERVICE_ID))
               .Returns(GetEscalationPolicy());

            _emailAdapaterMock.Setup(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_3));

            _timerAdapterMock.Setup(m => m.CreateTimer(MONITORED_SERVICE_ID, _timer15min));

            // Act
            var pager = new PagerService(_persistenceAdapterMock.Object, _escalationPolicyAdapterMock.Object, _timerAdapterMock.Object,
                _emailAdapaterMock.Object, _smsAdapaterMock.Object) as IPagerService;
            pager.AcknowledgmentTimeout(MONITORED_SERVICE_ID);

            // Assert
            _persistenceAdapterMock.Verify(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID), Times.Once);
            _persistenceAdapterMock.Verify(m => m.UpdateEscalationLevel(MONITORED_SERVICE_ID, ESCALATION_LEVEL_3), Times.Once);
            _escalationPolicyAdapterMock.Verify(m => m.RetrieveEscalationPolicy(SERVICE_ID), Times.Once);
            _emailAdapaterMock.Verify(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_3), Times.Once);
            _timerAdapterMock.Verify(m => m.CreateTimer(MONITORED_SERVICE_ID, _timer15min), Times.Once);
        }

        [Test]
        [Description("Checks that the correct number of targets is notified")]
        public void NotifyAllTargets()
        {
            // Set up
            _persistenceAdapterMock.Setup(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID))
                .Returns(GetMonitoredService(isHealthy: false, escalationLevel: ESCALATION_LEVEL_1));
            _persistenceAdapterMock.Setup(m => m.UpdateEscalationLevel(MONITORED_SERVICE_ID, ESCALATION_LEVEL_2));

            _escalationPolicyAdapterMock
               .Setup(m => m.RetrieveEscalationPolicy(SERVICE_ID))
               .Returns(GetEscalationPolicy());

            _emailAdapaterMock.Setup(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_1));
            _emailAdapaterMock.Setup(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_2));

            _smsAdapaterMock.Setup(m => m.SendSms(ESCALATION_LEVEL_SMS_1));
            _smsAdapaterMock.Setup(m => m.SendSms(ESCALATION_LEVEL_SMS_2));

            _timerAdapterMock.Setup(m => m.CreateTimer(MONITORED_SERVICE_ID, _timer15min));

            // Act
            var pager = new PagerService(_persistenceAdapterMock.Object, _escalationPolicyAdapterMock.Object, _timerAdapterMock.Object,
                _emailAdapaterMock.Object, _smsAdapaterMock.Object) as IPagerService;
            pager.AcknowledgmentTimeout(MONITORED_SERVICE_ID);

            // Assert
            _persistenceAdapterMock.Verify(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID), Times.Once);
            _persistenceAdapterMock.Verify(m => m.UpdateEscalationLevel(MONITORED_SERVICE_ID, ESCALATION_LEVEL_2), Times.Once);
            _escalationPolicyAdapterMock.Verify(m => m.RetrieveEscalationPolicy(SERVICE_ID), Times.Once);

            _emailAdapaterMock.Verify(m => m.SendEmail(It.IsAny<string>()), Times.Exactly(2));
            _emailAdapaterMock.Verify(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_1), Times.Once);
            _emailAdapaterMock.Verify(m => m.SendEmail(ESCALATION_LEVEL_EMAIL_2), Times.Once);

            _smsAdapaterMock.Verify(m => m.SendSms(It.IsAny<string>()), Times.Exactly(2));
            _smsAdapaterMock.Verify(m => m.SendSms(ESCALATION_LEVEL_SMS_1), Times.Once);
            _smsAdapaterMock.Verify(m => m.SendSms(ESCALATION_LEVEL_SMS_2), Times.Once);

            _timerAdapterMock.Verify(m => m.CreateTimer(MONITORED_SERVICE_ID, _timer15min), Times.Once);
        }

        [Test]
        [Description("Checks the correct behavior for a monitored service that is acknowledged")]
        public void MonitoredServiceAcknowledged()
        {
            // Set Up
            _persistenceAdapterMock.Setup(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID))
                .Returns(GetMonitoredService(isHealthy: false, escalationLevel: ESCALATION_LEVEL_2));
            _persistenceAdapterMock.Setup(m => m.Acknowledgement(MONITORED_SERVICE_ID));

            // Act
            var pager = new PagerService(_persistenceAdapterMock.Object, _escalationPolicyAdapterMock.Object, _timerAdapterMock.Object,
                _emailAdapaterMock.Object, _smsAdapaterMock.Object) as IPagerService;
            pager.Acknowledgment(MONITORED_SERVICE_ID);

            // Assert
            _persistenceAdapterMock.Verify(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID), Times.Once);
            _persistenceAdapterMock.Verify(m => m.Acknowledgement(MONITORED_SERVICE_ID), Times.Once);
        }

        [Test]
        [Description("Checks that there is no further notifications being sent after an acknowledgment")]
        public void NoNotificationOnceMonitoredServiceIsAcknowledged()
        {
            // Set Up
            _persistenceAdapterMock.Setup(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID))
                .Returns(GetMonitoredService(isHealthy: false, isAlertAcknowledged:true, escalationLevel: ESCALATION_LEVEL_2));

            _escalationPolicyAdapterMock
               .Setup(m => m.RetrieveEscalationPolicy(SERVICE_ID))
               .Returns(GetEscalationPolicy());

            // Act
            var pager = new PagerService(_persistenceAdapterMock.Object, _escalationPolicyAdapterMock.Object, _timerAdapterMock.Object,
                _emailAdapaterMock.Object, _smsAdapaterMock.Object) as IPagerService;
            pager.AcknowledgmentTimeout(MONITORED_SERVICE_ID);

            // Assert
            _persistenceAdapterMock.Verify(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID), Times.Once);
            _escalationPolicyAdapterMock.Verify(m => m.RetrieveEscalationPolicy(SERVICE_ID), Times.Once);
        }

        [Test]
        [Description("Checks the correct behavior when an additional alert is revived for the same service")]
        public void DuplicateAlertForMonitoredService()
        {
            //Set Up
            _persistenceAdapterMock.Setup(m => m.CreateOrRetrieveMonitoredService(SERVICE_ID))
                .Returns(GetMonitoredService(isHealthy: false));
            
            // Act
            var pager = new PagerService(_persistenceAdapterMock.Object, _escalationPolicyAdapterMock.Object, _timerAdapterMock.Object,
                _emailAdapaterMock.Object, _smsAdapaterMock.Object) as IPagerService;
            pager.Alert(GetAlert());

            // Assert
            _persistenceAdapterMock.Verify(m => m.CreateOrRetrieveMonitoredService(SERVICE_ID), Times.Once);
        }

        [Test]
        [Description("Checks that the monitored service is set to healthy after an healthy event")]
        public void HealthyEventFollowedByAcknowledgementTimeoutSetToHealthy()
        {
            // Set Up
            _persistenceAdapterMock.Setup(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID))
                .Returns(GetMonitoredService(isHealthy: false));
            _persistenceAdapterMock.Setup(m => m.UpdateHealthStatus(MONITORED_SERVICE_ID, true));

            //Act
            var pager = new PagerService(_persistenceAdapterMock.Object, _escalationPolicyAdapterMock.Object, _timerAdapterMock.Object,
                _emailAdapaterMock.Object, _smsAdapaterMock.Object) as IPagerService;
            pager.UpdateHealthStatus(GetHealthyEvent());
            pager.AcknowledgmentTimeout(MONITORED_SERVICE_ID);

            //Assert
            _persistenceAdapterMock.Verify(m => m.RetrieveMonitoredService(MONITORED_SERVICE_ID), Times.Once);
            _persistenceAdapterMock.Verify(m => m.UpdateHealthStatus(MONITORED_SERVICE_ID, true), Times.Once);

        }

        private EscalationPolicy GetEscalationPolicy()
        {
            return new EscalationPolicy
            {
                MonitoredServiceId = MONITORED_SERVICE_ID,
                Levels = new List<Level>
                {
                    new Level
                    {
                        EscalationLevel = ESCALATION_LEVEL_1,
                        Targets = new List<Target>
                        {
                            new Target
                            {
                                Type = TargetType.Email,
                                ContactInfo = ESCALATION_LEVEL_EMAIL_1
                            }
                        }
                    },
                    new Level
                    {
                        EscalationLevel = ESCALATION_LEVEL_2,
                        Targets = new List<Target>
                        {
                            new Target
                            {
                                Type = TargetType.Email,
                                ContactInfo = ESCALATION_LEVEL_EMAIL_2
                            },
                            new Target
                            {
                                Type = TargetType.Sms,
                                ContactInfo = ESCALATION_LEVEL_SMS_1
                            },
                            new Target
                            {
                                Type = TargetType.Sms,
                                ContactInfo = ESCALATION_LEVEL_SMS_2
                            },
                            new Target
                            {
                                Type = TargetType.Email,
                                ContactInfo = ESCALATION_LEVEL_EMAIL_1
                            },
                        }
                    },
                    new Level
                    {
                        EscalationLevel = ESCALATION_LEVEL_3,
                        Targets = new List<Target>
                        {
                            new Target
                            {
                                Type = TargetType.Email,
                                ContactInfo = ESCALATION_LEVEL_EMAIL_3
                            }
                        }
                    }
                }
            };
        }

        private AlertEvent GetAlert()
        {
            return new AlertEvent
            {
                ServiceId = SERVICE_ID,
                AlertMessage = "Error"
            };
        }

        private HealthStatusEvent GetHealthyEvent()
        {
            return new HealthStatusEvent
            {
                ServiceId = SERVICE_ID,
                IsHealthy = true
            };
        }

        private MonitoredService GetMonitoredService(
            int id = MONITORED_SERVICE_ID,
            bool isHealthy = true,
            int serviceId = SERVICE_ID,
            bool isAlertAcknowledged = false,
            int escalationLevel = ESCALATION_LEVEL_1)
        {
            return new MonitoredService
            {
                Id = id,
                IsHealthy = isHealthy,
                ServiceId = serviceId,
                IsAlertAcknowledged = isAlertAcknowledged,
                EscalationLevel = escalationLevel
            };
        }
    }
}
