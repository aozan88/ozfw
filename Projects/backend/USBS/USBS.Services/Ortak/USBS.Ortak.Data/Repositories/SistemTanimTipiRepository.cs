using OzFwBaseModels.Enums;
using Microsoft.EntityFrameworkCore;
using OzFw.BaseInfrastructure;
using USBS.Common.Domain;
using Ortak.Application.Contract;
using Ortak.Data.Persistence;

namespace Ortak.Data.Repositories
{
    public class SistemTanimTipiRepository : GenericRepository<SistemTanimTipi>, ISistemTanimTipiRepository
    {
        private readonly OrtakDbContext _context;

        public SistemTanimTipiRepository(OrtakDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SistemTanimTipi>> GetAktifTanimTipiListAsync()
        {
            return await _context.SistemTanimTipis.Where(t => t.Durum == DurumEnum.Aktif).ToListAsync();
        }
    }
}
