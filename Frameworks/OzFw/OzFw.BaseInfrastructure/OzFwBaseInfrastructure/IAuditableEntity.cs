namespace OzFw.BaseInfrastructure
{
    public interface IAuditableEntity
    {
        public string? OlusturanKisi { get; set; }
        public string? DegistirenKisi { get; set; }
        //public string Silen { get; set; }
        DateTime OlusturulmaTarihi { get; set; }
        DateTime DegistirilmeTarihi { get; set; }
        //DateTime SilinmeTarihi { get; set; }
    }
}
