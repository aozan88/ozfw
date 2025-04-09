using KonFw.BaseInfrastructure;
using KonFwBaseModels;
using USBS.Common.Domain;

namespace Ortak.Application.Contract
{
    public interface ISistemTanimTipiRepository : IRepository<SistemTanimTipi>
    {
        Task<IEnumerable<SistemTanimTipi>> GetAktifTanimTipiListAsync();
    }
}
