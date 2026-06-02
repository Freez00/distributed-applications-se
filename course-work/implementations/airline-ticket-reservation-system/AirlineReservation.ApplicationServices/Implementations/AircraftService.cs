using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Data.Entities;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public class AircraftService : EntityServiceBase<AircraftEntity, AircraftFilterRequest, AircraftCreateRequest, AircraftUpdateRequest, AircraftResponse>, IAircraftService
{
    public AircraftService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    protected override IRepository<AircraftEntity> Repository => UnitOfWork.Aircraft;

    protected override IQueryable<AircraftEntity> ApplyFilters(IQueryable<AircraftEntity> query, AircraftFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.RegistrationNumber))
        {
            query = query.Where(x => x.RegistrationNumber.Contains(filter.RegistrationNumber));
        }

        if (!string.IsNullOrWhiteSpace(filter.Model))
        {
            query = query.Where(x => x.Model.Contains(filter.Model));
        }

        if (!string.IsNullOrWhiteSpace(filter.Manufacturer))
        {
            query = query.Where(x => x.Manufacturer.Contains(filter.Manufacturer));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        return query;
    }

    protected override IQueryable<AircraftEntity> ApplySorting(IQueryable<AircraftEntity> query, AircraftFilterRequest filter)
        => (filter.SortBy.ToLowerInvariant(), filter.SortDirection) switch
        {
            ("registrationnumber", "desc") => query.OrderByDescending(x => x.RegistrationNumber),
            ("registrationnumber", _) => query.OrderBy(x => x.RegistrationNumber),
            ("model", "desc") => query.OrderByDescending(x => x.Model),
            ("model", _) => query.OrderBy(x => x.Model),
            ("seatcapacity", "desc") => query.OrderByDescending(x => x.SeatCapacity),
            ("seatcapacity", _) => query.OrderBy(x => x.SeatCapacity),
            _ when filter.SortDirection == "desc" => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

    protected override async Task BeforeCreateAsync(AircraftEntity entity, AircraftCreateRequest request, CancellationToken cancellationToken)
    {
        if (await UnitOfWork.Aircraft.Query().AnyAsync(x => x.RegistrationNumber == entity.RegistrationNumber, cancellationToken))
        {
            throw new InvalidOperationException("An aircraft with this registration number already exists.");
        }
    }

    protected override async Task BeforeUpdateAsync(AircraftEntity entity, AircraftUpdateRequest request, CancellationToken cancellationToken)
    {
        if (await UnitOfWork.Aircraft.Query().AnyAsync(x => x.Id != entity.Id && x.RegistrationNumber == entity.RegistrationNumber, cancellationToken))
        {
            throw new InvalidOperationException("An aircraft with this registration number already exists.");
        }
    }

    public override async Task<bool> ForceDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var aircraft = await UnitOfWork.Aircraft.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (aircraft is null)
        {
            return false;
        }

        var flights = await UnitOfWork.Flights.Query()
            .Where(x => x.AircraftId == id)
            .ToListAsync(cancellationToken);

        await RemoveReservationsForFlightsAsync(flights.Select(x => x.Id), cancellationToken);

        foreach (var flight in flights)
        {
            UnitOfWork.Flights.Remove(flight);
        }

        UnitOfWork.Aircraft.Remove(aircraft);
        await SaveDeleteChangesAsync(cancellationToken);
        return true;
    }

    protected override AircraftEntity MapCreateRequest(AircraftCreateRequest request) => new()
    {
        RegistrationNumber = request.RegistrationNumber.Trim().ToUpperInvariant(),
        Model = request.Model.Trim(),
        Manufacturer = request.Manufacturer.Trim(),
        SeatCapacity = request.SeatCapacity,
        RangeKm = request.RangeKm,
        ManufactureYear = request.ManufactureYear,
        IsActive = request.IsActive,
        LastMaintenanceDate = request.LastMaintenanceDate
    };

    protected override void ApplyUpdateRequest(AircraftEntity entity, AircraftUpdateRequest request)
    {
        entity.RegistrationNumber = request.RegistrationNumber.Trim().ToUpperInvariant();
        entity.Model = request.Model.Trim();
        entity.Manufacturer = request.Manufacturer.Trim();
        entity.SeatCapacity = request.SeatCapacity;
        entity.RangeKm = request.RangeKm;
        entity.ManufactureYear = request.ManufactureYear;
        entity.IsActive = request.IsActive;
        entity.LastMaintenanceDate = request.LastMaintenanceDate;
    }

    protected override AircraftResponse MapToResponse(AircraftEntity entity) => new()
    {
        Id = entity.Id,
        RegistrationNumber = entity.RegistrationNumber,
        Model = entity.Model,
        Manufacturer = entity.Manufacturer,
        SeatCapacity = entity.SeatCapacity,
        RangeKm = entity.RangeKm,
        ManufactureYear = entity.ManufactureYear,
        IsActive = entity.IsActive,
        LastMaintenanceDate = entity.LastMaintenanceDate,
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
