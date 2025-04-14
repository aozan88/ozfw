namespace KonFwBaseModels
{
    public class BaseRequestDto<T>
    {

        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public Guid ClientId { get; set; }
        public SearchFilter? Filter { get; set; }
        public T? Payload { get; set; }
        public required IEnumerable<Sort> Sort { get; set; }
    }
}
