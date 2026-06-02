using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Data.Entities;
using AirlineReservation.Data.Enums;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public class TicketService : EntityServiceBase<TicketEntity, TicketFilterRequest, TicketCreateRequest, TicketUpdateRequest, TicketResponse>, ITicketService
{
    public TicketService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    protected override IRepository<TicketEntity> Repository => UnitOfWork.Tickets;

    protected override IQueryable<TicketEntity> ApplyFilters(IQueryable<TicketEntity> query, TicketFilterRequest filter)
    {
        if (filter.ReservationId.HasValue)
        {
            query = query.Where(x => x.ReservationId == filter.ReservationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.PassengerLastName))
        {
            query = query.Where(x => x.PassengerLastName.Contains(filter.PassengerLastName));
        }

        if (!string.IsNullOrWhiteSpace(filter.PassengerDocumentNumber))
        {
            query = query.Where(x => x.PassengerDocumentNumber.Contains(filter.PassengerDocumentNumber));
        }

        if (!string.IsNullOrWhiteSpace(filter.FareClass))
        {
            var fareClass = ParseEnum<FareClass>(filter.FareClass);
            query = query.Where(x => x.FareClass == fareClass);
        }

        if (!string.IsNullOrWhiteSpace(filter.TicketStatus))
        {
            var status = ParseEnum<TicketStatus>(filter.TicketStatus);
            query = query.Where(x => x.TicketStatus == status);
        }

        return query;
    }

    protected override IQueryable<TicketEntity> ApplySorting(IQueryable<TicketEntity> query, TicketFilterRequest filter)
        => (filter.SortBy.ToLowerInvariant(), filter.SortDirection) switch
        {
            ("passengerlastname", "desc") => query.OrderByDescending(x => x.PassengerLastName),
            ("passengerlastname", _) => query.OrderBy(x => x.PassengerLastName),
            ("fareclass", "desc") => query.OrderByDescending(x => x.FareClass),
            ("fareclass", _) => query.OrderBy(x => x.FareClass),
            ("price", "desc") => query.OrderByDescending(x => x.Price),
            ("price", _) => query.OrderBy(x => x.Price),
            ("ticketstatus", "desc") => query.OrderByDescending(x => x.TicketStatus),
            ("ticketstatus", _) => query.OrderBy(x => x.TicketStatus),
            _ when filter.SortDirection == "desc" => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

    public override async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await UnitOfWork.Tickets.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (ticket is null)
        {
            return false;
        }

        var reservationId = ticket.ReservationId;
        UnitOfWork.Tickets.Remove(ticket);
        await SaveDeleteChangesAsync(cancellationToken);
        await RecalculateReservationAsync(reservationId, cancellationToken);
        return true;
    }

    protected override async Task BeforeCreateAsync(TicketEntity entity, TicketCreateRequest request, CancellationToken cancellationToken)
    {
        await ValidateReservationAsync(entity.ReservationId, cancellationToken);
        await ValidateSeatAsync(entity, cancellationToken);
    }

    protected override async Task BeforeUpdateAsync(TicketEntity entity, TicketUpdateRequest request, CancellationToken cancellationToken)
    {
        await ValidateReservationAsync(entity.ReservationId, cancellationToken);
        await ValidateSeatAsync(entity, cancellationToken);
    }

    protected override async Task AfterSaveAsync(TicketEntity entity, CancellationToken cancellationToken)
    {
        await RecalculateReservationAsync(entity.ReservationId, cancellationToken);
    }

    protected override TicketEntity MapCreateRequest(TicketCreateRequest request) => new()
    {
        ReservationId = request.ReservationId,
        PassengerFirstName = request.PassengerFirstName.Trim(),
        PassengerLastName = request.PassengerLastName.Trim(),
        PassengerDocumentNumber = request.PassengerDocumentNumber.Trim(),
        PassengerType = ParseEnum<PassengerType>(request.PassengerType),
        SeatNumber = string.IsNullOrWhiteSpace(request.SeatNumber) ? null : request.SeatNumber.Trim().ToUpperInvariant(),
        FareClass = ParseEnum<FareClass>(request.FareClass),
        Price = request.Price,
        Currency = request.Currency.Trim().ToUpperInvariant(),
        TicketStatus = ParseEnum<TicketStatus>(request.TicketStatus),
        IssuedAt = request.IssuedAt
    };

    protected override void ApplyUpdateRequest(TicketEntity entity, TicketUpdateRequest request)
    {
        entity.ReservationId = request.ReservationId;
        entity.PassengerFirstName = request.PassengerFirstName.Trim();
        entity.PassengerLastName = request.PassengerLastName.Trim();
        entity.PassengerDocumentNumber = request.PassengerDocumentNumber.Trim();
        entity.PassengerType = ParseEnum<PassengerType>(request.PassengerType);
        entity.SeatNumber = string.IsNullOrWhiteSpace(request.SeatNumber) ? null : request.SeatNumber.Trim().ToUpperInvariant();
        entity.FareClass = ParseEnum<FareClass>(request.FareClass);
        entity.Price = request.Price;
        entity.Currency = request.Currency.Trim().ToUpperInvariant();
        entity.TicketStatus = ParseEnum<TicketStatus>(request.TicketStatus);
        entity.IssuedAt = request.IssuedAt;
    }

    protected override TicketResponse MapToResponse(TicketEntity entity) => new()
    {
        Id = entity.Id,
        ReservationId = entity.ReservationId,
        PassengerFirstName = entity.PassengerFirstName,
        PassengerLastName = entity.PassengerLastName,
        PassengerDocumentNumber = entity.PassengerDocumentNumber,
        PassengerType = entity.PassengerType.ToString(),
        SeatNumber = entity.SeatNumber,
        FareClass = entity.FareClass.ToString(),
        Price = entity.Price,
        Currency = entity.Currency,
        TicketStatus = entity.TicketStatus.ToString(),
        IssuedAt = entity.IssuedAt
    };

    private async Task ValidateReservationAsync(int reservationId, CancellationToken cancellationToken)
    {
        if (!await UnitOfWork.Reservations.Query().AnyAsync(x => x.Id == reservationId, cancellationToken))
        {
            throw new InvalidOperationException("Reservation was not found.");
        }
    }

    private async Task ValidateSeatAsync(TicketEntity entity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity.SeatNumber))
        {
            return;
        }

        var reservation = await UnitOfWork.Reservations.Query().FirstAsync(x => x.Id == entity.ReservationId, cancellationToken);
        var seatTaken = await UnitOfWork.Tickets.Query()
            .Include(x => x.Reservation)
            .AnyAsync(x =>
                x.Id != entity.Id &&
                x.SeatNumber == entity.SeatNumber &&
                x.Reservation != null &&
                x.Reservation.FlightId == reservation.FlightId &&
                x.TicketStatus != TicketStatus.Cancelled &&
                x.TicketStatus != TicketStatus.Refunded,
                cancellationToken);

        if (seatTaken)
        {
            throw new InvalidOperationException("Seat number is already taken for this flight.");
        }
    }

    private async Task RecalculateReservationAsync(int reservationId, CancellationToken cancellationToken)
    {
        var reservation = await UnitOfWork.Reservations.Query()
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);

        if (reservation is null)
        {
            return;
        }

        ReservationService.RecalculateTotals(reservation);
        UnitOfWork.Reservations.Update(reservation);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
