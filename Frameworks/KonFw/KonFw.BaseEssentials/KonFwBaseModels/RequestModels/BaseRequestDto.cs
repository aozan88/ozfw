namespace KonFwBaseModels
{
    public class BaseRequestDto<T>
    {

        public int Page { get; set; } = 0;
        public int Size { get; set; } = 10;
        public Guid ClientId { get; set; }
        public SearchFilter? Filter { get; set; }
        public T? SearchValue;
        public required IEnumerable<Sort> Sort { get; set; }
    }
}
