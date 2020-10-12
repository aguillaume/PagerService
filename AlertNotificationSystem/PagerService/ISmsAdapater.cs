namespace AlertNotificationSystem.PagerService
{
    public interface ISmsAdapater
    {
        /// <summary>
        /// Sends an SMS to the specified phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to be sent the message</param>
        void SendSms(string phoneNumber);
    }
}