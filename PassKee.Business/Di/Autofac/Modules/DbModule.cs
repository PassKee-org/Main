using Autofac;
using Persistence.Transactions.Behaviors;
using PassKee.Orm.Connection;

namespace PassKee.Business.Di.Autofac.Modules
{
    public class DbModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DbConnectionFactory>()
                .As<IDbConnectionFactory>()
                .SingleInstance();

            builder
                .RegisterType<DbSessionProvider>()
                .As<IDbSessionProvider>()
                .InstancePerLifetimeScope();
        }
    }
}
