using KonFw.BaseInfrastructure;

namespace USBS.Common.Domain
{
    public class BaseEntity : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string Olusturan { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Guncelleyen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Silen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime OlusturulmaTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime DegistirilmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime SilinmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
