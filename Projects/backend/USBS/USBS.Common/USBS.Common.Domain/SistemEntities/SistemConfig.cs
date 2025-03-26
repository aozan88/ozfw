namespace USBS.Common.Domain
{
    public class SistemConfig : BaseEntity
    {
        public string SistemConfigAdi { get; set; }
        public Guid UstSistemConfigId { get; set; }
        public string SistemConfigDegerTipi { get; set; }
        public string SistemConfigSistemConfigu { get; set; }
        public string SistemConfigDeger { get; set; }
        public string Aciklama{ get; set; }
        public int Durum { get; set; }
        public bool? BagimlilikVarMi { get; set; }
    }
}
