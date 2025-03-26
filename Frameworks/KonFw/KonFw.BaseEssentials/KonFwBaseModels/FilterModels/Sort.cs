namespace KonFwBaseModels
{
    public class Sort
    {
        public required string Field { get; set; }
        public required SortType Dir { get; set; }
    }
    public enum SortType { 
        ASC,
        DESC
    }
}
