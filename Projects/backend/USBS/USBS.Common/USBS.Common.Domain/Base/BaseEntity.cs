using KonFw.BaseInfrastructure;
using KonFwBaseModels.Enums;

namespace USBS.Common.Domain
{
    public class BaseEntity : IEntity, IAuditableEntity, ISoftDeletable
    {
        public Guid Id { get; set; }
        public string? OlusturanKisi { get; set; }
        public string? DegistirenKisi { get; set; }
        //public string Silen { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
        public DateTime DegistirilmeTarihi { get; set; }
        //public DateTime SilinmeTarihi { get; set; }
        public int Versiyon { get; set; }
        public DurumEnum Durum { get; set; } = DurumEnum.Aktif;
        public string? LanguageTag { get; set; }
    }
}
