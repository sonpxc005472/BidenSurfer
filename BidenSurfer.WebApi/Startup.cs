using MassTransit;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;
using BidenSurfer.WebApi;
using BidenSurfer.WebApi.Services;
using BidenSurfer.WebApi.Consumers;

namespace BidenSurfer.WebApi
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
            services.AddScoped<AppDbContext>();           
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IConfigService, ConfigService>();
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddCustomMassTransit(Configuration);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("RedisConn");
                options.InstanceName = "BidenSurfer_ByBit_";
            });
            services.AddScoped<IRedisCacheService,RedisCacheService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting()
               .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });        
        }
    }

    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            void ConfigureEnpoint(IBusRegistrationContext ctx, IBusFactoryConfigurator cfg)
            {
                #region Subscribe endpoints

                cfg.ReceiveEndpoint(QueueName.SaveNewConfig, x =>
                {
                    x.Consumer<SaveNewConfigConsumer>(ctx);
                });
                cfg.ReceiveEndpoint(QueueName.OffConfigEvent, x =>
                {
                    x.Consumer<OffConfigConsumer>(ctx);
                });

                #endregion

                #region Publish endpoints

                EndpointConvention.Map<RestartBotMessage>(new Uri($"queue:{QueueName.RestartUserBot}"));

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

                    //cfg.UseMessageRetry(r => r.Incremental(5, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)));

                    ConfigureEnpoint(ctx, cfg);
                });
            });

            return services;
        }
    }
}
