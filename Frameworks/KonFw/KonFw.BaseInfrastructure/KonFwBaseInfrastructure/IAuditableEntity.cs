namespace KonFw.BaseInfrastructure
{
    public interface IAuditableEntity
    {
        public string Olusturan { get; set; }
        public string Guncelleyen { get; set; }
        public string Silen { get; set; }
        DateTime OlusturulmaTarihi { get; set; }
        DateTime DegistirilmeTarihi { get; set; }
        DateTime SilinmeTarihi { get; set; }
    }
}