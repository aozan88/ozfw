using USBS.Common.Domain;

namespace Ortak.Application.DtoModels
{
    public class KodDto : BaseDto
    {
        public string KodAdi { get; set; }
        public Guid KodTipiId { get; set; }
        public Guid? UstKodId { get; set; }
        public string KisaAd { get; set; }
        public string Aciklama { get; set; }
        public int Durum{ get; set; }
    }
}
