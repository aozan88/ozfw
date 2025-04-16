using KonFwBaseModels;

namespace Sistem.Application.DtoModels
{
    public class SistemParametreDto : BaseDto
    {
        public string SistemParametreAdi { get; set; }
        public Guid SistemParametreTipiId { get; set; }
        public Guid? UstSistemParametreId { get; set; }
        public string KisaAd { get; set; }
        public string Aciklama { get; set; }
    }
}
