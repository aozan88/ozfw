using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBS.Common.Dtos.BaseDtos
{
    public class BaseDto : IAuditableDto
    {
        public Guid Id { get; set; }
        public string Olusturan { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Guncelleyen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Silen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime OlusturulmaTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime DegistirilmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime SilinmeTarihi { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
