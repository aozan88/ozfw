namespace KonFwBaseModels
{
    public class HistoryDto
    {
        public Guid Id { get; set; }
        public string Entity { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
        public string Author{ get; set; }
        public string Date{ get; set; }
        public string Changes{ get; set; }
        public string ChangeValues{ get; set; }
    }
}
