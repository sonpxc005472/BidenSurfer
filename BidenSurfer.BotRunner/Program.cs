using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;
using System.Configuration;
using BidenSurfer.BotRunner.Consumers;
using BidenSurfer.Infras.Helpers;

namespace BidenSurfer.BotRunner
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
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true, true);
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
                services.AddSingleton<AppDbContext>();
                services.AddSingleton<ITeleMessage, TeleMessage>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IConfigService, ConfigService>();
                services.AddStackExchangeRedisCache(options =>
                {                    
                    options.Configuration = configuration.GetConnectionString("RedisConn");
                    options.InstanceName = "BidenSurfer_ByBit_";
                });
                services.AddCustomMassTransit(configuration);
                services.AddScoped<IRedisCacheService, RedisCacheService>();
                services.AddScoped<IBotService, BotService>();
                services.AddHostedService<AutoRunBotService>();
            });
    }

    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            void ConfigureEnpoint(IBusRegistrationContext ctx, IBusFactoryConfigurator cfg)
            {
                #region Subscribe endpoints

                cfg.ReceiveEndpoint(QueueName.ScannerIndicator, x =>
                {
                    x.Consumer<ScannerIndicatorConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.OnOffConfigMessageBotRunner, x =>
                {
                    x.Consumer<OnOffConfigConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.ConfigUpdateFromApiForBotRunnerMessage, x =>
                {
                    x.Consumer<ConfigUpdateFromApiConsumer>(ctx);
                });
                

                #endregion

                #region Publish endpoints

                EndpointConvention.Map<RunAllBotMessage>(new Uri($"queue:{QueueName.RunAllBot}"));
                EndpointConvention.Map<OffConfigMessage>(new Uri($"queue:{QueueName.OffConfigEvent}"));
                EndpointConvention.Map<AmountExpireMessage>(new Uri($"queue:{QueueName.AmountExpireMessage}"));
                EndpointConvention.Map<OnOffConfigMessageScanner>(new Uri($"queue:{QueueName.OnOffConfigMessageScanner}"));
                EndpointConvention.Map<UpdateConfigMessage>(new Uri($"queue:{QueueName.UpdateConfigMessage}"));

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

                    cfg.PrefetchCount = 16;

                    ConfigureEnpoint(ctx, cfg);
                });
            });

            return services;
        }
    }
}