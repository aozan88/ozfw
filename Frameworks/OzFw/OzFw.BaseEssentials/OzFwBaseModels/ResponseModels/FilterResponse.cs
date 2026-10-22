namespace OzFwBaseModels.ResponseModels
{
    // not used
    public class FilterResponse<T> : BaseResponseDto<T>
    {
        public IQueryable<T>? Query { get; set; }
        public int PageNumber { get; set; }
        public long Count { get; set; }
    }
}
