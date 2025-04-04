using USBS.Common.Dtos.BaseDtos;

namespace USBS.Common.Dtos.SistemDtos
{
    public class SistemParametreDto : BaseDto
    {
        public string ParametreAdi { get; set; }
        public Guid UstParametreId { get; set; }
        public Guid ParametreTipiId { get; set; }
        public string ParametreDegerTipi { get; set; }
        public string ParametreKodu { get; set; }
        public string ParametreDeger { get; set; }
        public string Aciklama { get; set; }
        public int Durum { get; set; }
        public bool? BagimlilikVarMi { get; set; }
    }
}
