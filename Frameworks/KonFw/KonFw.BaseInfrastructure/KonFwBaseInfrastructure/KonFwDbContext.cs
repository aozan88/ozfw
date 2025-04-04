using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KonFw.BaseInfrastructure
{
    public abstract class KonFwDbContext : DbContext
    {
        private readonly IHttpContextAccessor _context;
        public KonFwDbContext(DbContextOptions options, IHttpContextAccessor context) : base(options) {
            _context = context;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditableEntityChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditableEntityChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditableEntity &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                    entity.OlusturulmaTarihi = DateTime.UtcNow;

                if (entry.State == EntityState.Deleted)
                    entity.DegistirilmeTarihi = DateTime.UtcNow;
                    //entity.SilinmeTarihi= DateTime.UtcNow;

            }
        }
    }

}
