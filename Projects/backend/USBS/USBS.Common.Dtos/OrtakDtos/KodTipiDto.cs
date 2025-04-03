using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USBS.Common.Dtos.BaseDtos;

namespace USBS.Common.Dtos.OrtakDtos
{
    public class KodTipiDto : BaseDto
    {
        public string KodTipiAdi { get; set; }
        public int Durum { get; set; }
        public string Aciklama { get; set; }

    }
}
