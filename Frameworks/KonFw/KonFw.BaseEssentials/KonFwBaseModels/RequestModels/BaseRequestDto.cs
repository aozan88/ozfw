namespace KonFwBaseModels
{
    public class BaseRequestDto<T>
    {

        private int Page { get; set; } = 0;
        private int Size { get; set; } = 10;
        private Guid ClientId { get; set; }
        private SearchFilter? SearchFilter { get; set; }
        public T? ObjectValue;
    }
}
