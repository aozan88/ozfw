using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USBS.Common.Domain;

namespace DTOS.OrtakDtos
{
    public class KodDto : BaseDto
    {
        public string KodAdi { get; set; }
        public Guid KodTipiId { get; set; }
        public Guid? UstKodId { get; set; }
        public string KisaAd { get; set; }
        public string Aciklama { get; set; }
        public int Durum { get; set; }
    }
}
