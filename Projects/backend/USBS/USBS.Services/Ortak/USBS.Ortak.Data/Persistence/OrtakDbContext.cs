using KonFw.BaseInfrastructure;
using Microsoft.EntityFrameworkCore;
using USBS.Common.Domain;

namespace Ortak.Data.Persistence
{
    public class OrtakDbContext : KonFwDbContext
    {
        public OrtakDbContext(DbContextOptions<OrtakDbContext> options) : base(options) { }

        public DbSet<Kod> Kods { get; set; }
    }
}
