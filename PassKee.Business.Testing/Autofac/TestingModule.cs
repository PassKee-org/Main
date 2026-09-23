using Autofac;
using Domain.Abstractions;
using PassKee.Business.Testing.Factories;
using PassKee.Business.Testing.Seeders.Entity;

namespace PassKee.Business.Testing.Autofac;

public class TestingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(BusinessTestingAssemblyMarker).Assembly)
            .AsClosedTypesOf(typeof(IDataFactory<>))
            .InstancePerDependency();
        
        builder
            .RegisterAssemblyTypes(typeof(BusinessTestingAssemblyMarker).Assembly)
            .AssignableTo<IDomainService>()
            .AsImplementedInterfaces()
            .InstancePerDependency();
    }
}
