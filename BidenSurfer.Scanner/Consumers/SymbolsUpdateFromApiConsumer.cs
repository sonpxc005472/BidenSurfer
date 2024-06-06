using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Scanner.Services;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class SymbolsUpdateFromApiConsumer : IConsumer<SymbolInfoUpdateForScannerMessage>
    {
        private readonly IBotService _botService;
        public SymbolsUpdateFromApiConsumer(IBotService botService)
        {
            _botService = botService;
        }

        public async Task Consume(ConsumeContext<SymbolInfoUpdateForScannerMessage> context)
        {
            var symbols = context.Message?.Symbols;
            if (symbols != null && symbols.Any())
            {
                StaticObject.Symbols = symbols;
                await _botService.SubscribeSticker();
            }                        
        }
    }
}
