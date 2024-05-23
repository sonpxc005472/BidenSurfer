using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class SymbolsUpdateFromApiConsumer : IConsumer<SymbolInfoUpdateForScannerMessage>
    {
        public async Task Consume(ConsumeContext<SymbolInfoUpdateForScannerMessage> context)
        {
            var symbols = context.Message?.Symbols;
            if (symbols != null && symbols.Any())
            {
                StaticObject.Symbols = symbols;
            }                        
        }
    }
}
