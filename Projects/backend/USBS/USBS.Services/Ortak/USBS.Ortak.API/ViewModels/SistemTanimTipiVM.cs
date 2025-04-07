using USBS.Common.Dtos.BaseDtos;

namespace Ortak.API.ViewModels
{
    public class SistemTanimTipiVM : BaseVM
    {
        public string TanimTipiAdi { get; set; }
        public string Aciklama { get; set; }
        public int Sira { get; set; }
    }
}
