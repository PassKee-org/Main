using Notification.Abstractions;

namespace PassKee.Business.Notifications.Senders;

public class TestNotificationItemContext : INotificationItemContext
{
    public TestNotificationItemContext() {}

    public TestNotificationItemContext(string toAddress, string message = "Test notification")
    {
        ToAddress = toAddress;
        Message = message;
    }

    public string ToAddress { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
