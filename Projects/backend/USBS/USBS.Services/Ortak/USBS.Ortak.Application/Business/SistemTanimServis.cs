using Ortak.Application.Contract;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;
using KonFw.ServiceBase;

namespace Ortak.Application.Business
{
    public class SistemTanimServis : BaseService<SistemTanim, SistemTanimDto>, ISistemTanimServis
    {
        private readonly ISistemTanimRepository _repository;

        public SistemTanimServis(ISistemTanimRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
