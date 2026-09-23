using Autofac;
using Microsoft.Extensions.Configuration;
using PassKee.Api;
using PassKee.Business.Testing.Extensions;

namespace PassKee.Tests.Integration.Api;

public class TestStartup: Startup
{
    public TestStartup(IConfiguration configuration) : base(configuration)
    {
    }

    public override void ConfigureContainer(ContainerBuilder builder)
    {
        base.ConfigureContainer(builder);
        builder.ConfigureTestingScope();
    }
}
