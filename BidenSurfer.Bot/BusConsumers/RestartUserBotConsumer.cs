using MassTransit;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BidenSurfer.Bot.BusConsumers
{
    public class RestartUserBotConsumer : IConsumer<RestartBotMessage>
    {
        private readonly OkxDbContext _dbcontext;
        public RestartUserBotConsumer(OkxDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task Consume(ConsumeContext<RestartBotMessage> context)
        {
            var data = await _dbcontext.Configs.ToListAsync();
        }
    }
}
