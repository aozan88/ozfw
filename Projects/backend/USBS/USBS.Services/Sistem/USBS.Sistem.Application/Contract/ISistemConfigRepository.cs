using KonFwBaseInfrastructure;
using USBS.Common.Domain;

namespace Sistem.Application.Contract
{
    public interface ISistemConfigRepository : IRepository<SistemConfig>
    {
        Task<IEnumerable<SistemConfig>> GetActiveSistemConfigsAsync();
    }
}
