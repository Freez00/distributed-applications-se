using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Data.Entities;
using AirlineReservation.Data.Enums;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public class FlightService : EntityServiceBase<FlightEntity, FlightFilterRequest, FlightCreateRequest, FlightUpdateRequest, FlightResponse>, IFlightService
{
    public FlightService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    protected override IRepository<FlightEntity> Repository => UnitOfWork.Flights;

    protected override IQueryable<FlightEntity> BuildReadQuery()
        => UnitOfWork.Flights.Query()
            .AsNoTracking()
            .Include(x => x.Aircraft)
            .Include(x => x.Reservations)
                .ThenInclude(x => x.Tickets);

    public override async Task<FlightResponse> CreateAsync(FlightCreateRequest request, CancellationToken cancellationToken = default)
    {
        var created = await base.CreateAsync(request, cancellationToken);
        return await GetByIdAsync(created.Id, cancellationToken) ?? created;
    }

    public override async Task<FlightResponse?> UpdateAsync(int id, FlightUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var updated = await base.UpdateAsync(id, request, cancellationToken);
        return updated is null ? null : await GetByIdAsync(updated.Id, cancellationToken) ?? updated;
    }

    protected override IQueryable<FlightEntity> ApplyFilters(IQueryable<FlightEntity> query, FlightFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.FlightNumber))
        {
            query = query.Where(x => x.FlightNumber.Contains(filter.FlightNumber));
        }

        if (filter.DepartureAirportId.HasValue)
        {
            query = query.Where(x => x.DepartureAirportId == filter.DepartureAirportId.Value);
        }

        if (filter.ArrivalAirportId.HasValue)
        {
            query = query.Where(x => x.ArrivalAirportId == filter.ArrivalAirportId.Value);
        }

        if (filter.DepartureDate.HasValue)
        {
            var date = filter.DepartureDate.Value.Date;
            query = query.Where(x => x.DepartureTime.Date == date);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = ParseEnum<FlightStatus>(filter.Status);
            query = query.Where(x => x.Status == status);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => x.BasePrice >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => x.BasePrice <= filter.MaxPrice.Value);
        }

        return query;
    }

    protected override IQueryable<FlightEntity> ApplySorting(IQueryable<FlightEntity> query, FlightFilterRequest filter)
        => (filter.SortBy.ToLowerInvariant(), filter.SortDirection) switch
        {
            ("flightnumber", "desc") => query.OrderByDescending(x => x.FlightNumber),
            ("flightnumber", _) => query.OrderBy(x => x.FlightNumber),
            ("departuretime", "desc") => query.OrderByDescending(x => x.DepartureTime),
            ("departuretime", _) => query.OrderBy(x => x.DepartureTime),
            ("baseprice", "desc") => query.OrderByDescending(x => x.BasePrice),
            ("baseprice", _) => query.OrderBy(x => x.BasePrice),
            ("status", "desc") => query.OrderByDescending(x => x.Status),
            ("status", _) => query.OrderBy(x => x.Status),
            _ when filter.SortDirection == "desc" => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

    protected override async Task BeforeCreateAsync(FlightEntity entity, FlightCreateRequest request, CancellationToken cancellationToken)
    {
        await ValidateFlightAsync(entity, cancellationToken);
        if (await UnitOfWork.Flights.Query().AnyAsync(x => x.FlightNumber == entity.FlightNumber, cancellationToken))
        {
            throw new InvalidOperationException("A flight with this flight number already exists.");
        }
    }

    protected override async Task BeforeUpdateAsync(FlightEntity entity, FlightUpdateRequest request, CancellationToken cancellationToken)
    {
        await ValidateFlightAsync(entity, cancellationToken);
        if (await UnitOfWork.Flights.Query().AnyAsync(x => x.Id != entity.Id && x.FlightNumber == entity.FlightNumber, cancellationToken))
        {
            throw new InvalidOperationException("A flight with this flight number already exists.");
        }
    }

    public override async Task<bool> ForceDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var flight = await UnitOfWork.Flights.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (flight is null)
        {
            return false;
        }

        var reservations = await UnitOfWork.Reservations.Query()
            .Where(x => x.FlightId == id)
            .ToListAsync(cancellationToken);

        foreach (var reservation in reservations)
        {
            UnitOfWork.Reservations.Remove(reservation);
        }

        UnitOfWork.Flights.Remove(flight);
        await SaveDeleteChangesAsync(cancellationToken);
        return true;
    }

    protected override FlightEntity MapCreateRequest(FlightCreateRequest request) => new()
    {
        FlightNumber = request.FlightNumber.Trim().ToUpperInvariant(),
        DepartureAirportId = request.DepartureAirportId,
        ArrivalAirportId = request.ArrivalAirportId,
        AircraftId = request.AircraftId,
        DepartureTime = request.DepartureTime,
        ArrivalTime = request.ArrivalTime,
        BasePrice = request.BasePrice,
        Currency = request.Currency.Trim().ToUpperInvariant(),
        Status = ParseEnum<FlightStatus>(request.Status)
    };

    protected override void ApplyUpdateRequest(FlightEntity entity, FlightUpdateRequest request)
    {
        entity.FlightNumber = request.FlightNumber.Trim().ToUpperInvariant();
        entity.DepartureAirportId = request.DepartureAirportId;
        entity.ArrivalAirportId = request.ArrivalAirportId;
        entity.AircraftId = request.AircraftId;
        entity.DepartureTime = request.DepartureTime;
        entity.ArrivalTime = request.ArrivalTime;
        entity.BasePrice = request.BasePrice;
        entity.Currency = request.Currency.Trim().ToUpperInvariant();
        entity.Status = ParseEnum<FlightStatus>(request.Status);
    }

    protected override FlightResponse MapToResponse(FlightEntity entity) => new()
    {
        Id = entity.Id,
        FlightNumber = entity.FlightNumber,
        DepartureAirportId = entity.DepartureAirportId,
        ArrivalAirportId = entity.ArrivalAirportId,
        AircraftId = entity.AircraftId,
        DepartureTime = entity.DepartureTime,
        ArrivalTime = entity.ArrivalTime,
        BasePrice = entity.BasePrice,
        Currency = entity.Currency,
        Status = entity.Status.ToString(),
        AvailableSeats = CalculateAvailableSeats(entity),
        CreatedAt = entity.CreatedAt
    };

    private async Task ValidateFlightAsync(FlightEntity entity, CancellationToken cancellationToken)
    {
        if (entity.DepartureAirportId == entity.ArrivalAirportId)
        {
            throw new InvalidOperationException("Departure and arrival airports must be different.");
        }

        if (entity.ArrivalTime <= entity.DepartureTime)
        {
            throw new InvalidOperationException("Arrival time must be after departure time.");
        }

        if (!await UnitOfWork.Airports.Query().AnyAsync(x => x.Id == entity.DepartureAirportId, cancellationToken))
        {
            throw new InvalidOperationException("Departure airport was not found.");
        }

        if (!await UnitOfWork.Airports.Query().AnyAsync(x => x.Id == entity.ArrivalAirportId, cancellationToken))
        {
            throw new InvalidOperationException("Arrival airport was not found.");
        }

        if (!await UnitOfWork.Aircraft.Query().AnyAsync(x => x.Id == entity.AircraftId, cancellationToken))
        {
            throw new InvalidOperationException("Aircraft was not found.");
        }
    }

    internal static int CalculateAvailableSeats(FlightEntity flight)
    {
        var capacity = flight.Aircraft?.SeatCapacity ?? 0;
        var activeTickets = flight.Reservations
            .Where(x => x.Status is not ReservationStatus.Cancelled and not ReservationStatus.Expired)
            .SelectMany(x => x.Tickets)
            .Count(x => x.TicketStatus is TicketStatus.Issued or TicketStatus.CheckedIn);

        return Math.Max(0, capacity - activeTickets);
    }
}
