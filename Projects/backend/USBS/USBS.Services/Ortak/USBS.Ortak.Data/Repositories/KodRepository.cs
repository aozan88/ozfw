using Microsoft.EntityFrameworkCore;
using KonFw.BaseInfrastructure;
using USBS.Common.DbContexts;
using USBS.Common.Domain;
using Ortak.Application.Contract;
using Ortak.Data.Persistence;

namespace Ortak.Data.Repositories
{
    public class KodRepository : GenericRepository<Kod>, IKodRepository
    {
        private readonly OrtakDbContext _context;

        public KodRepository(OrtakDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Kod>> GetActiveKodsAsync()
        {
            return await _context.Kods.Where(k => k.Durum == 1).ToListAsync();
        }
    }
}
