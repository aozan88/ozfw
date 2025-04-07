using USBS.Common.Dtos.BaseDtos;

namespace USBS.Common.Dtos.OrtakDtos
{
    public class SistemTanimTipiDto : BaseDto
    {
        public string TanimTipiAdi { get; set; }
        public int Durum { get; set; }
        public string Aciklama { get; set; }
    }
}
