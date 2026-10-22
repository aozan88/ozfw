using OzFw.BaseInfrastructure;
using USBS.Common.Domain;

namespace Sistem.Application.Contract
{
    public interface ISistemParametreRepository : IRepository<SistemParametre>
    {
        Task<IEnumerable<SistemParametre>> GetActiveSistemParametresAsync();
    }
}
