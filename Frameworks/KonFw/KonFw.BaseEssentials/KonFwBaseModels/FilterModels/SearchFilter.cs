namespace KonFwBaseModels
{
    /// <summary>
    /// Filtre ağacı düğümü.
    /// Yaprak düğüm: Field + Operator + Value (eq, neq, lt, lte, gt, gte, contains, doesnotcontain,
    /// startswith, endswith, in, between, isnull, isnotnull).
    /// Grup düğümü: Logic ("and" | "or") + Filters.
    /// </summary>
    public class SearchFilter
    {
        public string? Field { get; set; }
        public string? Operator { get; set; }
        public object? Value { get; set; }
        public string? Logic { get; set; }
        public List<SearchFilter>? Filters { get; set; }
    }
}
