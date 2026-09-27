using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using PassKee.Api.Di.Autofac.Modules;
using PassKee.Api.Middleware;
using PassKee.Business;
using PassKee.Business.Extensions;
using PassKee.Business.Helpers;
using PassKee.Business.Mvc.Middleware;

namespace PassKee.Api;

public class Startup
{
    private readonly bool _isRequestResponseLoggingEnabled;

    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        _isRequestResponseLoggingEnabled = configuration.GetValue("App:EnableRequestResponseLogging", false);
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        var assembly = typeof(ApiAssemblyMarker).Assembly;
        services.AddAutoMapper(cfg => {}, assembly);
        services.AddCors(options =>
        {
            options.AddPolicy("Cors", policy =>
            {
                if (ApplicationHelper.HostingEnvironment == "Development")
                {
                    policy.WithOrigins(
                        // API
                        "https://dev.passkee.org",
                        "https://dev-api.passkee.org",
                        "https://localhost:7108",
                        "http://localhost:5265",
                        
                        // Web
                        "https://localhost:7230",
                        "http://localhost:5254",
                        "https://localhost:7091",
                        "http://localhost:5148",
                        "http://localhost:5000"
                    );
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        {
                            return uri.Host is "localhost" or "127.0.0.1"
                                   || uri.Host.EndsWith("passkee.org");
                        }
                        return false;
                    });
                }
                else
                {
                    policy.WithOrigins(
                        "https://passkee.org",
                        "https://api.passkee.org"
                    );   
                }

                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
        services.InitControllers(assembly);
        services.InitApiAuthServices(Configuration);
        
        // Disable X-Frame headers
        services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
    }

    public virtual void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterModule<ApiModule>()
            .RegisterAssemblyModules(typeof(BusinessAssemblyMarker).Assembly);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSerilogRequestLogging();
        }

        if (_isRequestResponseLoggingEnabled)
        {
            app.UseMiddleware<RequestResponseLoggerMiddleware>();
        }

        app.UseForwardedHeaders();
        app.UseRouting();
        app.UseCors("Cors");
        
        app.UseMiddleware<CommitPerformerMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
