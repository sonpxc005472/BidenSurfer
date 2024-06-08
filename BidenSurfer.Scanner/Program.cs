using BidenSurfer.Infras;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BidenSurfer.Scanner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Scanner.Consumers;
using BidenSurfer.Infras.Helpers;
using BidenSurfer.Infras.Loggers;
using BidenSurfer.BotRunner.Consumers;

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
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: true, true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .UseLogging("Scanner")
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
                services.AddCustomMassTransit(configuration);
                services.AddSingleton<ITeleMessage, TeleMessage>();
                services.AddScoped<IUserService, UserService>();
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
                #region Subscribe endpoints
                cfg.ReceiveEndpoint(QueueName.OnOffConfigMessageScanner, x =>
                {
                    x.Consumer<OnOffConfigConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.ScannerUpdateFromApiMessage, x =>
                {
                    x.Consumer<ScannerUpdateFromApiConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.ConfigUpdateFromApiForScannerMessage, x =>
                {
                    x.Consumer<ConfigUpdateFromApiConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.ScannerSettingUpdateFromApiMessage, x =>
                {
                    x.Consumer<ScannerSettingUpdateFromApiConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.UpdateUserForScannerMessage, x =>
                {
                    x.Consumer<UserUpdateFromApiConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.StartStopBotForScannerMessage, x =>
                {
                    x.Consumer<StartStopBotConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.StartStopScannerMessage, x =>
                {
                    x.Consumer<StartStopScannerConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.SymbolInfoUpdateForScannerMessage, x =>
                {
                    x.Consumer<SymbolsUpdateFromApiConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.ResetBotForScannerMessage, x =>
                {
                    x.Consumer<ResetScannerConsumer>(ctx);
                });

                #endregion

                #region Publish endpoints

                EndpointConvention.Map<NewConfigCreatedMessage>(new Uri($"queue:{QueueName.ScannerIndicator}"));
                EndpointConvention.Map<SaveNewConfigMessage>(new Uri($"queue:{QueueName.SaveNewConfig}"));
                EndpointConvention.Map<SendTeleMessage>(new Uri($"queue:{QueueName.SendTeleMessage}"));

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