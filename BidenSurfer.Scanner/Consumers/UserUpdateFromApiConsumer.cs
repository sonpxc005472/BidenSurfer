using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class UserUpdateFromApiConsumer : IConsumer<UpdateUserForScannerMessage>
    {
        private readonly IUserService _userService;
        public UserUpdateFromApiConsumer(IUserService userService)
        {
            _userService = userService;
        }
        public async Task Consume(ConsumeContext<UpdateUserForScannerMessage> context)
        {
            Console.WriteLine("UserUpdateFromApiConsumer...");
            await _userService.GetAllActive();            
        }
    }
}
