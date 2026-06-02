using AirlineReservation.Contracts.Common;

namespace AirlineReservation.ApplicationServices.Interfaces;

public interface ICrudService<TFilter, TCreate, TUpdate, TResponse>
    where TFilter : PagedRequest
{
    Task<PagedResult<TResponse>> GetAllAsync(TFilter filter, CancellationToken cancellationToken = default);

    Task<TResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TResponse> CreateAsync(TCreate request, CancellationToken cancellationToken = default);

    Task<TResponse?> UpdateAsync(int id, TUpdate request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ForceDeleteAsync(int id, CancellationToken cancellationToken = default);
}
