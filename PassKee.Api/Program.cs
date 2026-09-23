using Autofac.Extensions.DependencyInjection;
using Serilog;
using PassKee.Api;
using PassKee.Business.Helpers;
using PassKee.Business.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        using var log = LoggerInitializer.BuildSerilogInstance();
        Log.Logger = log;
            
        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception e)
        {
            log.Error(e, "Start application failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog(Log.Logger)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureAppConfiguration(config =>
            {
                config.ConfigureConfigurationProvider();
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
