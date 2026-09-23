using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PassKee.Business.Notifications.Senders;
using PassKee.Business.Services.Queue;
using PassKee.Orm.Constants;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api;

public class NotificationTests : BaseTest
{
    public NotificationTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldSendTestNotificationThroughQueue()
    {
        var context = new TestNotificationItemContext("test@example.com", "Hello from test");
        var queueService = ServiceProvider.GetRequiredService<IQueueService>();

        await queueService.PushNotificationAsync(context);
        await FlushDbChanges();

        var processedCount = await QueueProcess(QueueChannel.Notifications);
        Assert.True(processedCount >= 1);
        Assert.True(SmtpClientServiceMock.IsEmailSent);
        var sent = SmtpClientServiceMock.SentMessages.FirstOrDefault();
        Assert.NotNull(sent);
        Assert.Equal("test@example.com", sent.To);
    }
}

