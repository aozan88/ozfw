using System.Linq.Expressions;
using OzFwBaseHelpers;
using OzFwBaseModels;
using OzFwBaseModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace OzFw.BaseInfrastructure
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private static readonly bool IsSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(typeof(T));

        protected readonly DbContext Context;
        protected readonly DbSet<T> DbSet;

        public GenericRepository(DbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        /// <summary>Silinmiş kayıtlar hariç sorgu. Türetilmiş repository'ler özel sorgularına bununla başlamalı.</summary>
        protected IQueryable<T> Query(bool includeDeleted = false)
        {
            IQueryable<T> query = DbSet;
            return includeDeleted ? query : ExcludeDeleted(query);
        }

        public virtual async Task<T?> GetByIdAsync(object id, bool addIncludes = false)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null || !addIncludes)
                return entity;

            var entityType = Context.Model.FindEntityType(typeof(T));
            if (entityType != null)
            {
                foreach (var navigation in entityType.GetNavigations().Where(n => !n.IsCollection))
                {
                    await Context.Entry(entity).Reference(navigation.Name).LoadAsync();
                }
            }
            return entity;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Query().AsNoTracking().ToListAsync();
        }

        public virtual async Task<BaseResponseDto<T>> GetAllAsync<TRequest>(BaseRequestDto<TRequest> request, bool useSplitQuery = false, bool includeDeleted = false)
        {
            var query = Query(includeDeleted).AsNoTracking();
            if (useSplitQuery)
                query = query.AsSplitQuery();
            return await query.ApplyQuery(request);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await Query().AnyAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task AddManyAsync(IEnumerable<T> entities)
        {
            await DbSet.AddRangeAsync(entities);
            await Context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            AttachModified(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task UpdateManyAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                AttachModified(entity);
            }
            await Context.SaveChangesAsync();
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                return false;
            await DeleteAsync(entity);
            return true;
        }

        public virtual async Task DeleteAsync(T entity)
        {
            MarkDeleted(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task<int> DeleteByFilterAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await Query().Where(predicate).ToListAsync();
            foreach (var entity in entities)
            {
                MarkDeleted(entity);
            }
            await Context.SaveChangesAsync();
            return entities.Count;
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        private void AttachModified(T entity)
        {
            var entry = Context.Entry(entity);
            if (entry.State == EntityState.Detached)
                DbSet.Attach(entity);
            entry.State = EntityState.Modified;
        }

        private void MarkDeleted(T entity)
        {
            if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.Durum = DurumEnum.Silindi;
                AttachModified(entity);
            }
            else
            {
                DbSet.Remove(entity);
            }
        }

        private static IQueryable<T> ExcludeDeleted(IQueryable<T> query)
        {
            if (!IsSoftDeletable)
                return query;
            var parameter = Expression.Parameter(typeof(T), "x");
            var durum = Expression.Property(parameter, nameof(ISoftDeletable.Durum));
            var condition = Expression.NotEqual(durum, Expression.Constant(DurumEnum.Silindi));
            return query.Where(Expression.Lambda<Func<T, bool>>(condition, parameter));
        }
    }
}
