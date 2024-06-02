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

                Configure(loggerConfiguration, applicationName);
            });

            return hostBuilder;
        }

        private static void Configure(LoggerConfiguration loggerConfiguration, string applicationName)
        {
            loggerConfiguration
                .Filter.ByExcluding(m => applicationName == "WebApi" ? (!m.Properties["SourceContext"].ToString().Contains("Consumers.SendTeleMessageConsumer") && !m.Properties["SourceContext"].ToString().Contains("Consumers.AmountExpireConsumer") && !m.Properties["SourceContext"].ToString().Contains("ConfigService")) : string.IsNullOrEmpty(m.MessageTemplate.Text))
                .WriteTo.Seq("http://seq:5341");
        }
    }
}
