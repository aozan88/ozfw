using OzFw.BaseInfrastructure;
using OzFwBaseModels;
using USBS.Common.Domain;

namespace Ortak.Application.Contract
{
    public interface ISistemTanimRepository : IRepository<SistemTanim>
    {
        Task<IEnumerable<SistemTanim>> GetAktifTanimListAsync();
        Task<IEnumerable<SistemTanim>> GetAktifTanimByTanimTipiIdAsync(Guid tanimTipiId);
    }
}
