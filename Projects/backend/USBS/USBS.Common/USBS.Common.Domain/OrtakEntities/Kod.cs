namespace USBS.Common.Domain
{
    public class Kod : BaseEntity
    {
        public string KodAdi { get; set; }
        public Guid KodTipiId { get; set; }
        public Guid UstKodId { get; set; }
        public string KisaAd { get; set; }
        public string KodNo { get; set; }
        public int Durum { get; set; }
        public string Aciklama{ get; set; }

        public virtual KodTipi KodTipi {  get; set; }   
    }
}
