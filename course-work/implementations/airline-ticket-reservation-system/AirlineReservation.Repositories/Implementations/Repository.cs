using System.Linq.Expressions;
using AirlineReservation.Data;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Repositories.Implementations;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _set;

    public Repository(IDbContext context)
    {
        _set = context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query()
        => _set.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _set.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _set.ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await _set.Where(predicate).ToListAsync(cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity)
        => _set.Update(entity);

    public void Remove(TEntity entity)
        => _set.Remove(entity);
}
