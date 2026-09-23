using Autofac;
using Domain.Abstractions;
using PassKee.Business.Notifications;

namespace PassKee.Business.Di.Autofac.Modules
{
    public class QueueModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(typeof(BusinessAssemblyMarker).Assembly)
                .AsClosedTypesOf(typeof(IAsyncQueueHandler<>))
                .InstancePerDependency();
            
            builder
                .RegisterAssemblyTypes(typeof(BusinessNotificationsAssemblyMarker).Assembly)
                .AsClosedTypesOf(typeof(IAsyncQueueHandler<>))
                .InstancePerDependency();
        }
    }
}
