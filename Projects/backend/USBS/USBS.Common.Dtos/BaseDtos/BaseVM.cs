namespace USBS.Common.Dtos.BaseDtos
{
    public class BaseVM
    {
        public Guid Id { get; set; }
        public string OlusturanKisi { get; set; }
        public string DegistirenKisi { get; set; }
        //public string Silen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime OlusturulmaTarihi { get; set; }
        public DateTime DegistirilmeTarihi { get; set; }
        //public DateTime SilinmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Versiyon { get; set; }
        public int Durum { get; set; }
        public string LanguageTag { get; set; }
    }
}
