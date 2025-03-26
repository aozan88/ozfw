namespace Ortak.API.ViewModels
{
    public class KodVM
    {
        public string KodAdi { get; set; }
        public Guid KodTipiId { get; set; }
        public string KisaAd { get; set; }
        public string Aciklama { get; set; }
        public int Durum { get; set; }
    }
}
