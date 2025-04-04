namespace USBS.Common.Dtos.BaseDtos
{
    public class BaseDto : IAuditableDto
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
