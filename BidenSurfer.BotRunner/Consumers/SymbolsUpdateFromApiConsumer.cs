using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.BotRunner.Consumers
{
    public class SymbolsUpdateFromApiConsumer : IConsumer<SymbolInfoUpdateForBotRunnerMessage>
    {
        public async Task Consume(ConsumeContext<SymbolInfoUpdateForBotRunnerMessage> context)
        {
            var symbols = context.Message?.Symbols;
            if (symbols != null && symbols.Any())
            {
                StaticObject.Symbols = symbols;
            }                        
        }
    }
}
