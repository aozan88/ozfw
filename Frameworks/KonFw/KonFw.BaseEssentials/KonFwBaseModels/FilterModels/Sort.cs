namespace KonFwBaseModels
{
    public class Sort
    {
        public required string Field { get; set; }
        public SortType Dir { get; set; } = SortType.ASC;
    }
    // JSON'da "ASC"/"DESC" ya da 0/1 kabul edilir
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum SortType {
        ASC,
        DESC
    }
}
