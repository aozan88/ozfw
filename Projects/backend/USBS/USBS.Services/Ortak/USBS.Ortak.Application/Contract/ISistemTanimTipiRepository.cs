using KonFw.BaseInfrastructure;
using USBS.Common.Domain;

namespace Ortak.Application.Contract
{
    public interface ISistemTanimTipiRepository : IRepository<SistemTanimTipi>
    {
        Task<IEnumerable<SistemTanimTipi>> GetAktifTanimTipiListAsync();
    }
}
