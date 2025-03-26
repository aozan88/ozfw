using USBS.Common.Domain;

namespace Sistem.Application.DtoModels
{
    public class SistemConfigDto : BaseDto
    {
        public string SistemConfigAdi { get; set; }
        public Guid SistemConfigTipiId { get; set; }
        public Guid? UstSistemConfigId { get; set; }
        public string KisaAd { get; set; }
        public string Aciklama { get; set; }
        public int Durum{ get; set; }
    }
}
