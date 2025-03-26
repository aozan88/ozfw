namespace KonFwBaseModels
{
    public class SearchFilter
    {
        public required string Field { get; set; }
        public required string Operator { get; set; }
        public required object Value { get; set; }
        public required string Logic { get; set; }
        public required List<SearchFilter> Filters { get; set; }
    }
}
