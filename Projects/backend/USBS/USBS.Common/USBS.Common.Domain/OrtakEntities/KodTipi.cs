namespace USBS.Common.Domain
{
    public class KodTipi : BaseEntity
    {
        public string KodTipiAdi { get; set; }
        public int Durum { get; set; }
        public string Aciklama{ get; set; }

        public virtual List<Kod> KodList { get; set; } = new List<Kod>();
    }
}
