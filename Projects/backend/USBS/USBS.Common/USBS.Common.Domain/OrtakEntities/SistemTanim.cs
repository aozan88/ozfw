namespace USBS.Common.Domain
{
    public class SistemTanim : BaseEntity
    {
        public string TanimAdi { get; set; }
        public Guid TanimTipiId { get; set; }
        public Guid UstTanimId { get; set; }
        public string KisaAd { get; set; }
        public string TanimKodu { get; set; }
        public int Sira { get; set; }
        public string Aciklama{ get; set; }

        public virtual SistemTanimTipi TanimTipi {  get; set; }   
    }
}
