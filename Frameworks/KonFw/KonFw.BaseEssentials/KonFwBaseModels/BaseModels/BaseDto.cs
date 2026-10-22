using KonFwBaseModels.Enums;

namespace KonFwBaseModels
{
    public class BaseDto : IAuditableDto
    {
        public Guid Id { get; set; }
        public string? OlusturanKisi { get; set; }
        public string? DegistirenKisi { get; set; }
        //public string Silen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime OlusturulmaTarihi { get; set; }
        public DateTime DegistirilmeTarihi { get; set; }
        //public DateTime SilinmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Versiyon { get; set; }
        public DurumEnum Durum { get; set; } = DurumEnum.Aktif;
        public string? LanguageTag { get; set; }
    }
}
