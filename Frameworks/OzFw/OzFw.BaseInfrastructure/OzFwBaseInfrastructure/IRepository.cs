using System.Linq.Expressions;
using OzFwBaseModels;

namespace OzFw.BaseInfrastructure
{
    public interface IRepository<T> where T : class
    {
        /// <param name="addIncludes">true ise referans navigation property'leri de yüklenir</param>
        Task<T?> GetByIdAsync(object id, bool addIncludes = false);
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Filtre ağacı + sıralama + sayfalama. Alan adları TRequest (DTO) üzerinden entity'ye eşlenir.
        /// ISoftDeletable entity'lerde silinmiş kayıtlar includeDeleted=false iken dahil edilmez.
        /// </summary>
        Task<BaseResponseDto<T>> GetAllAsync<TRequest>(BaseRequestDto<TRequest> request, bool useSplitQuery = false, bool includeDeleted = false);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task AddManyAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateManyAsync(IEnumerable<T> entities);

        /// <summary>ISoftDeletable ise Durum = Silindi yapar, değilse kaydı siler. Kayıt yoksa false döner.</summary>
        Task<bool> DeleteAsync(Guid id);
        Task DeleteAsync(T entity);

        /// <summary>Koşula uyan kayıtları siler (soft-delete). Etkilenen kayıt sayısını döner.</summary>
        Task<int> DeleteByFilterAsync(Expression<Func<T, bool>> predicate);

        Task<int> SaveChangesAsync();
    }
}
