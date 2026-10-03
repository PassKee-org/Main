using Autofac;
using PassKee.Business.Clients.Smtp;
using PassKee.Business.Common.Services.Web.ReCaptcha;

namespace PassKee.Business.Testing.Extensions;

public static class ContainerBuilderExtensions
{
    public static void ConfigureTestingScope(this ContainerBuilder builder)
    {
        builder.RegisterAssemblyModules(
            typeof(BusinessAssemblyMarker).Assembly,
            typeof(BusinessTestingAssemblyMarker).Assembly
        );
        builder.RegisterType<FakeReCaptchaService>().As<IReCaptchaService>().InstancePerDependency();
        builder.RegisterType<SmtpClientServiceMock>().As<ISmtpClientService>().InstancePerLifetimeScope();
        builder.RegisterType<PassKee.Business.Testing.Services.GarageClientMock>().As<PassKee.Business.Services.Storage.Client.IFileStorageGarageClient>().SingleInstance();
    }
}
