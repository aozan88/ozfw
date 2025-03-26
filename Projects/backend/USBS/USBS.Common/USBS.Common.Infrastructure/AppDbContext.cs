using KonFw.BaseInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace USBS.Common.DbContexts
{
    public class AppDbContext : KonFwDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
