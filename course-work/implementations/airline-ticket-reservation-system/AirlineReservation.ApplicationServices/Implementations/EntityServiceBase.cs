using AirlineReservation.Contracts.Common;
using AirlineReservation.Data.Entities;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public abstract class EntityServiceBase<TEntity, TFilter, TCreate, TUpdate, TResponse>
    where TEntity : BaseEntity
    where TFilter : PagedRequest
{
    protected EntityServiceBase(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    protected IUnitOfWork UnitOfWork { get; }

    protected abstract IRepository<TEntity> Repository { get; }

    public virtual async Task<PagedResult<TResponse>> GetAllAsync(TFilter filter, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(filter);
        var query = ApplySorting(ApplyFilters(BuildReadQuery(), normalized), normalized);
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalized.PageNumber - 1) * normalized.PageSize)
            .Take(normalized.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            PageNumber = normalized.PageNumber,
            PageSize = normalized.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)normalized.PageSize)
        };
    }

    public virtual async Task<TResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await BuildReadQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? default : MapToResponse(entity);
    }

    public virtual async Task<TResponse> CreateAsync(TCreate request, CancellationToken cancellationToken = default)
    {
        var entity = MapCreateRequest(request);
        await BeforeCreateAsync(entity, request, cancellationToken);
        await Repository.AddAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        await AfterSaveAsync(entity, cancellationToken);
        return MapToResponse(entity);
    }

    public virtual async Task<TResponse?> UpdateAsync(int id, TUpdate request, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return default;
        }

        ApplyUpdateRequest(entity, request);
        await BeforeUpdateAsync(entity, request, cancellationToken);
        Repository.Update(entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        await AfterSaveAsync(entity, cancellationToken);
        return MapToResponse(entity);
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        Repository.Remove(entity);
        await SaveDeleteChangesAsync(cancellationToken);
        return true;
    }

    public virtual Task<bool> ForceDeleteAsync(int id, CancellationToken cancellationToken = default)
        => DeleteAsync(id, cancellationToken);

    protected virtual IQueryable<TEntity> BuildReadQuery()
        => Repository.Query().AsNoTracking();

    protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TFilter filter)
        => query;

    protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TFilter filter)
        => filter.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(x => x.Id)
            : query.OrderBy(x => x.Id);

    protected virtual Task BeforeCreateAsync(TEntity entity, TCreate request, CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected virtual Task BeforeUpdateAsync(TEntity entity, TUpdate request, CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected virtual Task AfterSaveAsync(TEntity entity, CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected abstract TEntity MapCreateRequest(TCreate request);

    protected abstract void ApplyUpdateRequest(TEntity entity, TUpdate request);

    protected abstract TResponse MapToResponse(TEntity entity);

    protected async Task SaveDeleteChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "This record cannot be deleted because it is used by related records. Use force delete to remove related records too.",
                ex);
        }
    }

    protected static TEnum ParseEnum<TEnum>(string value) where TEnum : struct
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Invalid {typeof(TEnum).Name} value: {value}");
    }

    private static TFilter Normalize(TFilter filter)
    {
        filter.PageNumber = Math.Max(1, filter.PageNumber);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);
        filter.SortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "id" : filter.SortBy;
        filter.SortDirection = filter.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
        return filter;
    }
}
