using Autofac;
using Notification.Abstractions;
using PassKee.Business.Clients.Smtp;
using PassKee.Business.Notifications;

namespace PassKee.Business.Di.Autofac.Modules;

public class NotificationsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(BusinessNotificationsAssemblyMarker).Assembly)
            .AsClosedTypesOf(typeof(IAsyncNotification<>))
            .InstancePerLifetimeScope();

        builder
            .RegisterType<ScopedAsyncNotificationFactory>()
            .As<IAsyncNotificationFactory>()
            .InstancePerLifetimeScope();

        builder
            .RegisterType<DefaultAsyncNotificationBuilder>()
            .As<IAsyncNotificationBuilder>()
            .InstancePerLifetimeScope();
        
        builder
            .RegisterType<SmtpClientService>()
            .As<ISmtpClientService>()
            .SingleInstance();
    }
}
