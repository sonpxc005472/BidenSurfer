using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;


namespace BidenSurfer.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((ctx, options) =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;
                        options.Listen(IPAddress.Any, 5501);
                        options.Listen(IPAddress.Any, 15501, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                    });

                    webBuilder.UseStartup<Startup>()
                        .UseKestrel(o =>
                        {
                            o.AllowSynchronousIO = true;
                        });
                });
    }
}
