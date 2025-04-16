namespace USBS.Common.Domain
{
    public class SistemParametre : BaseEntity
    {
        public string ParametreAdi { get; set; }
        public Guid UstParametreId { get; set; }
        public Guid ParametreTipiId { get; set; }
        public string ParametreDegerTipi { get; set; }
        public string ParametreKodu { get; set; }
        public string ParametreDeger { get; set; }
        public string Aciklama{ get; set; }
        public bool? BagimlilikVarMi { get; set; }
    }
}
