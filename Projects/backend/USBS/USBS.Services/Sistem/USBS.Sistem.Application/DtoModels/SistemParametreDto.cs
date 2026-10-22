using KonFwBaseModels;

namespace Sistem.Application.DtoModels
{
    public class SistemParametreDto : BaseDto
    {
        [EntityField(nameof(USBS.Common.Domain.SistemParametre.ParametreAdi))]
        public string SistemParametreAdi { get; set; }

        [EntityField(nameof(USBS.Common.Domain.SistemParametre.ParametreTipiId))]
        public Guid SistemParametreTipiId { get; set; }

        [EntityField(nameof(USBS.Common.Domain.SistemParametre.UstParametreId))]
        public Guid? UstSistemParametreId { get; set; }

        public string KisaAd { get; set; }
        public string Aciklama { get; set; }
    }
}
