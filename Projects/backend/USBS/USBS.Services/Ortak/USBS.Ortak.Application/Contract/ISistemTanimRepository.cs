using KonFw.BaseInfrastructure;
using USBS.Common.Domain;

namespace Ortak.Application.Contract
{
    public interface ISistemTanimRepository : IRepository<SistemTanim>
    {
        Task<IEnumerable<SistemTanim>> GetAktifTanimListAsync();
    }
}
