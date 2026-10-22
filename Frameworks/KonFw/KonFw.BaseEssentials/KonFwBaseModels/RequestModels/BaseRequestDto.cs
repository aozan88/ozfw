namespace KonFwBaseModels
{
    public class BaseRequestDto<T>
    {

        /// <summary>1 tabanlı sayfa numarası.</summary>
        public int Page { get; set; } = 1;
        /// <summary>Sayfa boyutu. -1 tüm kayıtlar.</summary>
        public int Size { get; set; } = 10;
        public Guid ClientId { get; set; }
        public SearchFilter? Filter { get; set; }
        public T? Payload { get; set; }
        public IEnumerable<Sort>? Sort { get; set; }
    }
}
