using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using PassKee.Business.Helpers;
using PassKee.Business.Helpers.Tests;

namespace PassKee.Business.Logging;

public static class LoggerInitializer
{
    public static LoggerConfiguration GetSerilogBuilder(bool isEnableInitLogging = true)
    {
        var environment = ApplicationHelper.HostingEnvironment;
        var configuration = ApplicationHelper.BuildConfiguration();
        var logBuilder = new LoggerConfiguration().ReadFrom.Configuration(configuration);

        if (UnitTestDetector.IsRunningFromXUnit)
        {
            return logBuilder;
        }

        if (isEnableInitLogging)
        {
            Log.Information("Init Serilog configuration for {HostingEnvironment} environment", environment);
        }

        var grayLogHost = configuration.GetValue<string>("App:Logging:GrayLog:Host");
        var grayLogPort = configuration.GetValue<int>("App:Logging:GrayLog:Port", 0);
        
        logBuilder.Enrich.WithProperty("Environment", environment);
        logBuilder.Enrich.WithProperty("AppName", configuration.GetValue<string>("App:Name"));

        // Gray log
        var isGrayLogEnabled = !string.IsNullOrWhiteSpace(grayLogHost);
        if (isGrayLogEnabled)
        {
            logBuilder.WriteTo.Graylog(new GraylogSinkOptions
            {
                HostnameOrAddress = grayLogHost,
                Port = grayLogPort,
                MinimumLogEventLevel = LogEventLevel.Information,
                TransportType = TransportType.Udp
            });
        }

        return logBuilder;
    }

    public static Logger BuildSerilogInstance()
    {
        return GetSerilogBuilder().CreateLogger();
    }
}
