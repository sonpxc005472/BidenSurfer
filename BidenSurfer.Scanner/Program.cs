using BidenSurfer.Infras;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BidenSurfer.Scanner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;
using BidenSurfer.Infras.Domains;
using BidenSurfer.BotRunner.Services;

namespace BidenSurfer.Scanner
{
    internal class Program
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
            .ConfigureServices((hostContext, services) =>
            {
                var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                IConfiguration configuration = builder.Build();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(false);
                });
                services.AddScoped<AppDbContext>();
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("RedisConn");
                    options.InstanceName = "BidenSurfer_ByBit_";
                });
                services.AddCustomMassTransit(configuration);
                services.AddScoped<IRedisCacheService, RedisCacheService>();
                services.AddScoped<IConfigService, ConfigService>();
                services.AddScoped<IScannerService, ScannerService>();
                services.AddScoped<IBotService, BotService>();
                services.AddHostedService<AutoRunService>();
            });
    }

    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            void ConfigureEnpoint(IBusRegistrationContext ctx, IBusFactoryConfigurator cfg)
            {                
                #region Publish endpoints

                EndpointConvention.Map<NewConfigCreatedMessage>(new Uri($"queue:{QueueName.ScannerIndicator}"));
                EndpointConvention.Map<SaveNewConfigMessage>(new Uri($"queue:{QueueName.SaveNewConfig}"));

                #endregion
            }

            services.AddMassTransit(x =>
            {
                x.AddConsumersFromNamespaceContaining<Anchor>();
                var messageBusOptions = configuration.GetOptions<RabbitMqOptions>("MessageBus");
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(new Uri(messageBusOptions.Url), "/", hc =>
                    {
                        hc.Username(messageBusOptions.UserName);
                        hc.Password(messageBusOptions.Password);
                    });

                    cfg.PrefetchCount = 10;

                    ConfigureEnpoint(ctx, cfg);
                });
            });

            return services;
        }
    }
}