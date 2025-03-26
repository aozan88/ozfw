using KonFwBaseInfrastructure;

namespace KonFw.BaseInfrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
    }
}
