using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace OzFw.BaseInfrastructure
{
    public abstract class OzFwDbContext : DbContext
    {
        private readonly IHttpContextAccessor _context;

        public OzFwDbContext(DbContextOptions options, IHttpContextAccessor context) : base(options) {
            _context = context;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditableEntityChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditableEntityChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditableEntityChanges()
        {
            var now = DateTime.UtcNow;
            var user = _context?.HttpContext?.User?.Identity?.Name;

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditableEntity &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.OlusturulmaTarihi = now;
                    entity.OlusturanKisi ??= user;
                }
                else
                {
                    entity.DegistirilmeTarihi = now;
                    entity.DegistirenKisi = user ?? entity.DegistirenKisi;
                    // Oluşturma bilgileri güncellemede ezilmesin
                    foreach (var name in new[] { nameof(IAuditableEntity.OlusturulmaTarihi), nameof(IAuditableEntity.OlusturanKisi) })
                    {
                        if (entry.Metadata.FindProperty(name) != null)
                            entry.Property(name).IsModified = false;
                    }
                }
            }
        }
    }
}
