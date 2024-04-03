using MassTransit;
using BidenSurfer.Bot.BusConsumers;
using BidenSurfer.Bot.Services;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;

namespace BidenSurfer.Bot
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.EnableDetailedErrors(true);
                options.EnableSensitiveDataLogging(false);
            });
            services.AddSingleton<AppDbContext>();
            services.AddScoped<IConfigService, ConfigService>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("RedisConn");
                options.InstanceName = "BidenSurfer_ByBit_";
            });
            services.AddCustomMassTransit(Configuration);
            services.AddScoped<IRedisCacheService, RedisCacheService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBotService, BotService>();
            services.AddHostedService<AutoRunBotService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();        
        }
    }

    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            void ConfigureEnpoint(IBusRegistrationContext ctx, IBusFactoryConfigurator cfg)
            {
                #region Subscribe endpoints

                cfg.ReceiveEndpoint(QueueName.RunAllBot, x =>
                {
                    x.Consumer<RunAllBotConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.RestartUserBot, x =>
                {
                    x.Consumer<RestartUserBotConsumer>(ctx);
                });

                #endregion

                #region Publish endpoints
                                
                EndpointConvention.Map<RunAllBotMessage>(new Uri($"queue:{QueueName.RunAllBot}"));
                
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
