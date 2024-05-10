using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.BotRunner.Consumers
{
    public class UserUpdateFromApiConsumer : IConsumer<UpdateUserForBotRunnerMessage>
    {
        private readonly IUserService _userService;
        public UserUpdateFromApiConsumer(IUserService userService)
        {
            _userService = userService;
        }
        public async Task Consume(ConsumeContext<UpdateUserForBotRunnerMessage> context)
        {
            await _userService.GetAllActive();            
        }
    }
}
