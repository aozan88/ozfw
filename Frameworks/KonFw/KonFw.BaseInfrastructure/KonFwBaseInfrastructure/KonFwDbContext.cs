using Microsoft.EntityFrameworkCore;

namespace KonFw.BaseInfrastructure
{
    public abstract class KonFwDbContext : DbContext
    {
        public KonFwDbContext(DbContextOptions options) : base(options) { }

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
                    entity.SilinmeTarihi= DateTime.UtcNow;

                entity.DegistirilmeTarihi = DateTime.UtcNow;
            }
        }
    }

}
