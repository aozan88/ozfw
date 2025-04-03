using USBS.Common.Domain;

namespace USBS.Dtos.OrtakDtos
{
    public class KodTipiDto : BaseDto
    {
        public string KodTipiAdi { get; set; }
        public int Durum { get; set; }
        public string Aciklama { get; set; }

    }
}
