using MassTransit;

namespace Tiendasp.Shared.Events
{

    [ExcludeFromTopology]
    public interface IRabbitEvent
    {
        public Guid EventId { get;  }
        public DateTime CreatedAt { get; }
    }
}
