namespace KonFwBaseModels
{
    public class BaseResponseDto<T>
    {

        public bool IsSuccess { get; set; }
        public string Mesaj { get; set; }
        public int PageNumber { get; set; } = 0;
        public IQueryable<T>? Query { get; set; }
        public long Count { get; set; }
        public int Size { get; set; } = 10;
        public long TotalRecordCount { get; set; }
        public Guid? ClientId { get; set; }

        public T? ObjectValue;
    }
}
