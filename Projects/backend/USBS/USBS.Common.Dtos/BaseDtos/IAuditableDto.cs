using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBS.Common.Dtos.BaseDtos
{
    public interface IAuditableDto
    {
        public string Olusturan { get; set; }
        public string Guncelleyen { get; set; }
        public string Silen { get; set; }
        DateTime OlusturulmaTarihi { get; set; }
        DateTime DegistirilmeTarihi { get; set; }
        DateTime SilinmeTarihi { get; set; }
    }
}
