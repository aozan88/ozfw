using Microsoft.EntityFrameworkCore;
using USBS.Common.DbContexts;
using USBS.Common.Domain;

namespace Ortak.Data.Persistence
{
    public class OrtakDbContext : AppDbContext
    {
        public OrtakDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Kod> Kods { get; set; }
    }
}
