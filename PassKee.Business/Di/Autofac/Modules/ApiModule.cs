using Autofac;
using Domain.Abstractions.Api;
using Microsoft.AspNetCore.Http;
using PassKee.Business.Services.Http;

namespace PassKee.Business.Di.Autofac.Modules;

public class ApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<HttpContextAccessor>()
            .As<IHttpContextAccessor>()
            .InstancePerLifetimeScope();
        
        builder
            .RegisterType<BaseApiRequestService>()
            .As<IBaseApiRequestService>()
            .InstancePerLifetimeScope();
    }
}
