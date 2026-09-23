using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PassKee.Business;
using PassKee.Business.Notifications;

namespace PassKee.WorkerServices;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<DummyLogWorker>();
        services.AddHostedService<Services.Queue.DefaultProcessingHostedService>();
        services.AddHostedService<Services.Queue.NotificationProcessingHostedService>();
    }

    public void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterAssemblyModules(typeof(BusinessAssemblyMarker).Assembly);
        containerBuilder.RegisterAssemblyModules(typeof(BusinessNotificationsAssemblyMarker).Assembly);
    }
    
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
    }
}
