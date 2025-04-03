namespace KonFwBaseModels
{
    // not used
    public class FilterDTO
    {
        public required IEnumerable<Sort> Sort { get; set; }
        public required SearchFilter  Filter { get; set; }
        public required int Page { get; set; } = 1;
        public required int Size { get; set; } = 10;
        public required string Key { get; set; }
    }
}
