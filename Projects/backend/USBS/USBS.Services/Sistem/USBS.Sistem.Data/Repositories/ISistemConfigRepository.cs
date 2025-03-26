using Microsoft.EntityFrameworkCore;
using KonFw.BaseInfrastructure;
using USBS.Common.DbContexts;
using USBS.Common.Domain;
using Sistem.Application.Contract;
using Sistem.Data.Persistence;

namespace Sistem.Data.Repositories
{
    public class SistemConfigRepository : GenericRepository<SistemConfig>, ISistemConfigRepository
    {
        private readonly SistemDbContext _context;

        public SistemConfigRepository(SistemDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SistemConfig>> GetActiveSistemConfigsAsync()
        {
            return await _context.SistemConfigs.Where(k => k.Durum == 1).ToListAsync();
        }
    }
}
