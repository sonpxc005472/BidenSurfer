using MassTransit;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BidenSurfer.Bot.BusConsumers
{
    public class StartStopUserBotConsumer : IConsumer<StopOrStartBotMessage>
    {
        private readonly OkxDbContext _dbcontext;
        public StartStopUserBotConsumer(OkxDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task Consume(ConsumeContext<StopOrStartBotMessage> context)
        {
            var data = await _dbcontext.Configs.ToListAsync();
        }
    }
}
