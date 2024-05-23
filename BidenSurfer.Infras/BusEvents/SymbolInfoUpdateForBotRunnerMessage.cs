using Bybit.Net.Objects.Models.V5;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class SymbolInfoUpdateForBotRunnerMessage : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public List<BybitSpotSymbol> Symbols { get; set; }
    }
}
