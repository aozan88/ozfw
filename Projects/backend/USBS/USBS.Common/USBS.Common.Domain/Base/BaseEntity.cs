using KonFw.BaseInfrastructure;

namespace USBS.Common.Domain
{
    public class BaseEntity : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string OlusturanKisi { get; set; }
        public string DegistirenKisi { get; set; }
        //public string Silen { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
        public DateTime DegistirilmeTarihi { get; set; }
        //public DateTime SilinmeTarihi { get; set; }
        public int Versiyon { get; set; }
        public int Durum { get; set; }
        public string LanguageTag { get; set; }
    }
}
