using System;
using System.Collections.Generic;
using System.Text;

namespace AlertNotificationSystem.PagerService
{
    public interface IEmailAdapater
    {
        /// <summary>
        /// Sends an email to the specified email address
        /// </summary>
        /// <param name="email">The email address to send the notification to</param>
        void SendEmail(string email);
    }
}
