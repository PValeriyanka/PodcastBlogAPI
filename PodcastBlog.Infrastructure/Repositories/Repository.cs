using Microsoft.EntityFrameworkCore;
using PodcastBlog.Domain.Interfaces.Repositories;

namespace PodcastBlog.Infrastructure.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly PodcastBlogContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(PodcastBlogContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}