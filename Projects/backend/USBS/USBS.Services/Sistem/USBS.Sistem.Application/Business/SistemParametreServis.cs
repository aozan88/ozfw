using Sistem.Application.Contract;
using Sistem.Application.DtoModels;
using USBS.Common.Domain;
using KonFw.ServiceBase;

namespace Sistem.Application.Business
{
    public class SistemParametreServis : BaseService<SistemParametre, SistemParametreDto>, ISistemParametreServis
    {
        private readonly ISistemParametreRepository _repository;

        public SistemParametreServis(ISistemParametreRepository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
