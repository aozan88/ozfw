using KonFwBaseInfrastructure;
using USBS.Common.Domain;

namespace Ortak.Application.Contract
{
    public interface IKodRepository : IRepository<Kod>
    {
        Task<IEnumerable<Kod>> GetActiveKodsAsync();
    }
}
