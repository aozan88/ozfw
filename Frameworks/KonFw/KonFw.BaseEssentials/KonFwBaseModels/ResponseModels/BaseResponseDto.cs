namespace KonFwBaseModels
{
    public class BaseResponseDto<T>
    {

        public bool IsSuccess { get; set; }
        public string Mesaj { get; set; }
        public int PageNumber { get; set; } = 0;
        public long Count { get; set; }
        public int Size { get; set; } = 10;
        public long TotalRecordCount { get; set; }
        public long TotalTableCount { get; set; }
        public Guid? ClientId { get; set; }
        public IEnumerable<T>? ObjectValue { get; set; }
        public List<FilterDTO> Filters { get; set; }
        public List<Sort> SortList { get; set; }
        public List<String> FieldNames { get; set; }
    }
}
