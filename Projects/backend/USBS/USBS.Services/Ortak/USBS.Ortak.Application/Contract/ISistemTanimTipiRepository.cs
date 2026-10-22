using OzFw.BaseInfrastructure;
using OzFwBaseModels;
using USBS.Common.Domain;

namespace Ortak.Application.Contract
{
    public interface ISistemTanimTipiRepository : IRepository<SistemTanimTipi>
    {
        Task<IEnumerable<SistemTanimTipi>> GetAktifTanimTipiListAsync();
    }
}
