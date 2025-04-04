namespace Ortak.API.ViewModels
{
    public class SistemTanimVM
    {
        public string TanimAdi { get; set; }
        public Guid TanimTipiId { get; set; }
        public string KisaAd { get; set; }
        public string TanimKodu { get; set; }
        public string Aciklama { get; set; }
        public int Durum { get; set; }
        public int Sira { get; set; }
    }
}
