namespace KonFwEventBus
{
    public interface IEvent
    {
        Guid Id { get; }
        DateTime CreationDate { get; }
    }

    public abstract class Event : IEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime CreationDate { get; } = DateTime.UtcNow;
    }
}
