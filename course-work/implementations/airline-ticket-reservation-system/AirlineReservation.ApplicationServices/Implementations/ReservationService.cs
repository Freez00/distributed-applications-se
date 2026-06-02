using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Data.Entities;
using AirlineReservation.Data.Enums;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public class ReservationService : EntityServiceBase<ReservationEntity, ReservationFilterRequest, ReservationCreateRequest, ReservationUpdateRequest, ReservationResponse>, IReservationService
{
    public ReservationService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    protected override IRepository<ReservationEntity> Repository => UnitOfWork.Reservations;

    protected override IQueryable<ReservationEntity> BuildReadQuery()
        => UnitOfWork.Reservations.Query().AsNoTracking().Include(x => x.Tickets);

    public override async Task<ReservationResponse> CreateAsync(ReservationCreateRequest request, CancellationToken cancellationToken = default)
    {
        var userExists = await UnitOfWork.Users.Query().AnyAsync(x => x.Id == request.UserId && x.IsActive, cancellationToken);
        if (!userExists)
        {
            throw new InvalidOperationException("User was not found or is inactive.");
        }

        var flight = await UnitOfWork.Flights.Query()
            .Include(x => x.Aircraft)
            .Include(x => x.Reservations)
                .ThenInclude(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == request.FlightId, cancellationToken);

        if (flight is null)
        {
            throw new InvalidOperationException("Flight was not found.");
        }

        if (flight.Status == FlightStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot create a reservation for a cancelled flight.");
        }

        if (FlightService.CalculateAvailableSeats(flight) < request.Tickets.Count)
        {
            throw new InvalidOperationException("The flight does not have enough available seats.");
        }

        var reservation = new ReservationEntity
        {
            UserId = request.UserId,
            FlightId = request.FlightId,
            ReservationCode = GenerateReservationCode(),
            BookingDate = DateTime.UtcNow,
            Currency = flight.Currency,
            Status = ReservationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        foreach (var draft in request.Tickets)
        {
            var ticket = new TicketEntity
            {
                PassengerFirstName = draft.PassengerFirstName.Trim(),
                PassengerLastName = draft.PassengerLastName.Trim(),
                PassengerDocumentNumber = draft.PassengerDocumentNumber.Trim(),
                PassengerType = ParseEnum<PassengerType>(draft.PassengerType),
                SeatNumber = string.IsNullOrWhiteSpace(draft.SeatNumber) ? null : draft.SeatNumber.Trim().ToUpperInvariant(),
                FareClass = ParseEnum<FareClass>(draft.FareClass),
                Currency = flight.Currency,
                TicketStatus = TicketStatus.Issued,
                IssuedAt = DateTime.UtcNow
            };

            ticket.Price = CalculateTicketPrice(flight.BasePrice, ticket.PassengerType, ticket.FareClass);
            reservation.Tickets.Add(ticket);
        }

        RecalculateTotals(reservation);
        await UnitOfWork.Reservations.AddAsync(reservation, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(reservation);
    }

    protected override IQueryable<ReservationEntity> ApplyFilters(IQueryable<ReservationEntity> query, ReservationFilterRequest filter)
    {
        if (filter.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == filter.UserId.Value);
        }

        if (filter.FlightId.HasValue)
        {
            query = query.Where(x => x.FlightId == filter.FlightId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.ReservationCode))
        {
            query = query.Where(x => x.ReservationCode.Contains(filter.ReservationCode));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = ParseEnum<ReservationStatus>(filter.Status);
            query = query.Where(x => x.Status == status);
        }

        if (filter.BookingDateFrom.HasValue)
        {
            query = query.Where(x => x.BookingDate >= filter.BookingDateFrom.Value);
        }

        if (filter.BookingDateTo.HasValue)
        {
            query = query.Where(x => x.BookingDate <= filter.BookingDateTo.Value);
        }

        return query;
    }

    protected override IQueryable<ReservationEntity> ApplySorting(IQueryable<ReservationEntity> query, ReservationFilterRequest filter)
        => (filter.SortBy.ToLowerInvariant(), filter.SortDirection) switch
        {
            ("reservationcode", "desc") => query.OrderByDescending(x => x.ReservationCode),
            ("reservationcode", _) => query.OrderBy(x => x.ReservationCode),
            ("bookingdate", "desc") => query.OrderByDescending(x => x.BookingDate),
            ("bookingdate", _) => query.OrderBy(x => x.BookingDate),
            ("totalprice", "desc") => query.OrderByDescending(x => x.TotalPrice),
            ("totalprice", _) => query.OrderBy(x => x.TotalPrice),
            ("status", "desc") => query.OrderByDescending(x => x.Status),
            ("status", _) => query.OrderBy(x => x.Status),
            _ when filter.SortDirection == "desc" => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

    protected override ReservationEntity MapCreateRequest(ReservationCreateRequest request)
        => throw new NotSupportedException("Use the custom CreateAsync implementation for reservations.");

    protected override void ApplyUpdateRequest(ReservationEntity entity, ReservationUpdateRequest request)
    {
        entity.Status = ParseEnum<ReservationStatus>(request.Status);
        entity.ExpiresAt = request.ExpiresAt;
    }

    protected override ReservationResponse MapToResponse(ReservationEntity entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        FlightId = entity.FlightId,
        ReservationCode = entity.ReservationCode,
        BookingDate = entity.BookingDate,
        TotalPrice = entity.TotalPrice,
        Currency = entity.Currency,
        Status = entity.Status.ToString(),
        PassengerCount = entity.PassengerCount,
        ExpiresAt = entity.ExpiresAt
    };

    internal static void RecalculateTotals(ReservationEntity reservation)
    {
        var activeTickets = reservation.Tickets
            .Where(x => x.TicketStatus is not TicketStatus.Cancelled and not TicketStatus.Refunded)
            .ToList();

        reservation.PassengerCount = activeTickets.Count;
        reservation.TotalPrice = activeTickets.Sum(x => x.Price);
    }

    private static decimal CalculateTicketPrice(decimal basePrice, PassengerType passengerType, FareClass fareClass)
    {
        var classMultiplier = fareClass switch
        {
            FareClass.PremiumEconomy => 1.35m,
            FareClass.Business => 2.25m,
            FareClass.First => 3.50m,
            _ => 1.00m
        };

        var passengerMultiplier = passengerType switch
        {
            PassengerType.Child => 0.75m,
            PassengerType.Student => 0.85m,
            PassengerType.Infant => 0.20m,
            _ => 1.00m
        };

        return Math.Round(basePrice * classMultiplier * passengerMultiplier, 2);
    }

    private static string GenerateReservationCode()
        => Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
}
