namespace USBS.Common.Domain
{
    public class SistemTanimTipi : BaseEntity
    {
        public string TanimTipiAdi { get; set; }
        public int Durum { get; set; }
        public string Aciklama{ get; set; }

        public virtual List<SistemTanim> SistemTanims { get; set; } = new List<SistemTanim>();
    }
}
