using Microsoft.Extensions.Configuration;
using Serilog;
using PassKee.Business.Common.Utils;

namespace PassKee.Business.Helpers
{
    public static class ApplicationHelper
    {
        public static string HostingEnvironment
        {
            get
            {
                var value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                return string.IsNullOrEmpty(value) ? "Development" : value;
            }

        }
        
        public static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .ConfigureConfigurationProvider()
                .Build();
        }

        public static IConfigurationBuilder ConfigureConfigurationProvider(this IConfigurationBuilder builder)
        {
            Log.Logger.Information($"Initializing configuration with \"{HostingEnvironment}\" environment");
            var basePath = AssemblyUtils.GetAssemblyPath();
            return builder.SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
#if DEBUG
                .AddJsonFile("appsettings.Debug.json", true, true)
#endif
                .AddJsonFile($"appsettings.{HostingEnvironment}.json", true)
                .AddJsonFile($"appsettings.Testing.json", optional: true)
                .AddJsonFile($"appsettings.Local.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
