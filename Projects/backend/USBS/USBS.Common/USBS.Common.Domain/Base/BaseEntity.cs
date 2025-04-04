using KonFw.BaseInfrastructure;

namespace USBS.Common.Domain
{
    public class BaseEntity : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string OlusturanKisi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DegistirenKisi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public string Silen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime OlusturulmaTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime DegistirilmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public DateTime SilinmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Versiyon { get; set; }
        public int Durum { get; set; }
        public string LanguageTag { get; set; }
    }
}
