using Microsoft.Extensions.Hosting;
using Serilog;

namespace BidenSurfer.Infras.Loggers
{
    public static class Extensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder, string applicationName = "BidenSurfer")
        {
            hostBuilder.UseSerilog((context, loggerConfiguration) =>
            {                
                loggerConfiguration
                    .Enrich.WithProperty("ApplicationName", applicationName);

                Configure(loggerConfiguration);
            });

            return hostBuilder;
        }

        private static void Configure(LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.WriteTo.Seq("http://seq:5341");
        }
    }
}
