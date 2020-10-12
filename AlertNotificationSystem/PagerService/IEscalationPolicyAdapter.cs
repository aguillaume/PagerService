using System;
using System.Collections.Generic;
using System.Text;

namespace AlertNotificationSystem.PagerService
{
    public interface IEscalationPolicyAdapter
    {
        /// <summary>
        /// Gets the escalation policy level and targets for the given service
        /// </summary>
        /// <param name="serviceId">The service id associated with the escalation policy</param>
        /// <returns>The escalation policy <see cref="EscalationPolicy"/></returns>
        EscalationPolicy RetrieveEscalationPolicy(int serviceId);
    }
}
