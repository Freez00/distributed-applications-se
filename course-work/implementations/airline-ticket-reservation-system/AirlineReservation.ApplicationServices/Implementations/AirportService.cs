using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Data.Entities;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public class AirportService : EntityServiceBase<AirportEntity, AirportFilterRequest, AirportCreateRequest, AirportUpdateRequest, AirportResponse>, IAirportService
{
    public AirportService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    protected override IRepository<AirportEntity> Repository => UnitOfWork.Airports;

    protected override IQueryable<AirportEntity> ApplyFilters(IQueryable<AirportEntity> query, AirportFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Code))
        {
            query = query.Where(x => x.Code.Contains(filter.Code));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(x => x.City.Contains(filter.City));
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            query = query.Where(x => x.Country.Contains(filter.Country));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        return query;
    }

    protected override IQueryable<AirportEntity> ApplySorting(IQueryable<AirportEntity> query, AirportFilterRequest filter)
        => (filter.SortBy.ToLowerInvariant(), filter.SortDirection) switch
        {
            ("code", "desc") => query.OrderByDescending(x => x.Code),
            ("code", _) => query.OrderBy(x => x.Code),
            ("city", "desc") => query.OrderByDescending(x => x.City),
            ("city", _) => query.OrderBy(x => x.City),
            ("country", "desc") => query.OrderByDescending(x => x.Country),
            ("country", _) => query.OrderBy(x => x.Country),
            _ when filter.SortDirection == "desc" => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

    protected override async Task BeforeCreateAsync(AirportEntity entity, AirportCreateRequest request, CancellationToken cancellationToken)
    {
        if (await UnitOfWork.Airports.Query().AnyAsync(x => x.Code == entity.Code, cancellationToken))
        {
            throw new InvalidOperationException("An airport with this code already exists.");
        }
    }

    protected override async Task BeforeUpdateAsync(AirportEntity entity, AirportUpdateRequest request, CancellationToken cancellationToken)
    {
        if (await UnitOfWork.Airports.Query().AnyAsync(x => x.Id != entity.Id && x.Code == entity.Code, cancellationToken))
        {
            throw new InvalidOperationException("An airport with this code already exists.");
        }
    }

    public override async Task<bool> ForceDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var airport = await UnitOfWork.Airports.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (airport is null)
        {
            return false;
        }

        var flights = await UnitOfWork.Flights.Query()
            .Where(x => x.DepartureAirportId == id || x.ArrivalAirportId == id)
            .ToListAsync(cancellationToken);

        await RemoveReservationsForFlightsAsync(flights.Select(x => x.Id), cancellationToken);

        foreach (var flight in flights)
        {
            UnitOfWork.Flights.Remove(flight);
        }

        UnitOfWork.Airports.Remove(airport);
        await SaveDeleteChangesAsync(cancellationToken);
        return true;
    }

    protected override AirportEntity MapCreateRequest(AirportCreateRequest request) => new()
    {
        Code = request.Code.Trim().ToUpperInvariant(),
        Name = request.Name.Trim(),
        City = request.City.Trim(),
        Country = request.Country.Trim(),
        Timezone = request.Timezone.Trim(),
        IsActive = request.IsActive
    };

    protected override void ApplyUpdateRequest(AirportEntity entity, AirportUpdateRequest request)
    {
        entity.Code = request.Code.Trim().ToUpperInvariant();
        entity.Name = request.Name.Trim();
        entity.City = request.City.Trim();
        entity.Country = request.Country.Trim();
        entity.Timezone = request.Timezone.Trim();
        entity.IsActive = request.IsActive;
    }

    protected override AirportResponse MapToResponse(AirportEntity entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        City = entity.City,
        Country = entity.Country,
        Timezone = entity.Timezone,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };

    private async Task RemoveReservationsForFlightsAsync(IEnumerable<int> flightIds, CancellationToken cancellationToken)
    {
        var ids = flightIds.ToArray();
        if (ids.Length == 0)
        {
            return;
        }

        var reservations = await UnitOfWork.Reservations.Query()
            .Where(x => ids.Contains(x.FlightId))
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            UnitOfWork.Reservations.Remove(reservation);
        }
    }
}
