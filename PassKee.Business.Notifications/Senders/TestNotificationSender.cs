using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using Notification.Abstractions;
using PassKee.Business.Clients.Smtp;
using PassKee.Business.Notifications.Core;

namespace PassKee.Business.Notifications.Senders;

public class TestNotificationSender : IAsyncQueueHandler<TestNotificationItemContext>, IAsyncNotification<TestNotificationItemContext>
{
    private readonly ISmtpClientService _smtpClientService;
    private readonly EmailFactory _emailFactory;

    public TestNotificationSender(
        ISmtpClientService smtpClientService
    )
    {
        _smtpClientService = smtpClientService;
        _emailFactory = new EmailFactory();
    }

    public Task HandleAsync(
        TestNotificationItemContext context, 
        CancellationToken cancellationToken = default
    )
    {
        var emailBuilder = _emailFactory.GetEmailBuilder("TestNotification.htm");
        _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        return Task.CompletedTask;
    }

    public Task SendAsync(
        TestNotificationItemContext context, 
        CancellationToken cancellationToken = default
    )
    {
        return HandleAsync(context, cancellationToken);
    }
}
