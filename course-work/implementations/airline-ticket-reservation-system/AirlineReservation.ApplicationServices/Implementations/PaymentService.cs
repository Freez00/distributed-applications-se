using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Data.Entities;
using AirlineReservation.Data.Enums;
using AirlineReservation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.ApplicationServices.Implementations;

public class PaymentService : EntityServiceBase<PaymentEntity, PaymentFilterRequest, PaymentCreateRequest, PaymentUpdateRequest, PaymentResponse>, IPaymentService
{
    public PaymentService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    protected override IRepository<PaymentEntity> Repository => UnitOfWork.Payments;

    protected override IQueryable<PaymentEntity> ApplyFilters(IQueryable<PaymentEntity> query, PaymentFilterRequest filter)
    {
        if (filter.ReservationId.HasValue)
        {
            query = query.Where(x => x.ReservationId == filter.ReservationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
        {
            var status = ParseEnum<PaymentStatus>(filter.PaymentStatus);
            query = query.Where(x => x.PaymentStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.PaymentMethod))
        {
            var method = ParseEnum<PaymentMethod>(filter.PaymentMethod);
            query = query.Where(x => x.PaymentMethod == method);
        }

        if (!string.IsNullOrWhiteSpace(filter.TransactionReference))
        {
            query = query.Where(x => x.TransactionReference != null && x.TransactionReference.Contains(filter.TransactionReference));
        }

        return query;
    }

    protected override IQueryable<PaymentEntity> ApplySorting(IQueryable<PaymentEntity> query, PaymentFilterRequest filter)
        => (filter.SortBy.ToLowerInvariant(), filter.SortDirection) switch
        {
            ("amount", "desc") => query.OrderByDescending(x => x.Amount),
            ("amount", _) => query.OrderBy(x => x.Amount),
            ("paymentstatus", "desc") => query.OrderByDescending(x => x.PaymentStatus),
            ("paymentstatus", _) => query.OrderBy(x => x.PaymentStatus),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", _) => query.OrderBy(x => x.CreatedAt),
            _ when filter.SortDirection == "desc" => query.OrderByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Id)
        };

    protected override async Task BeforeCreateAsync(PaymentEntity entity, PaymentCreateRequest request, CancellationToken cancellationToken)
    {
        await ValidateReservationPaymentAsync(entity.ReservationId, entity.Amount, entity.Currency, currentPaymentId: null, cancellationToken);
    }

    protected override async Task BeforeUpdateAsync(PaymentEntity entity, PaymentUpdateRequest request, CancellationToken cancellationToken)
    {
        await ValidateReservationPaymentAsync(entity.ReservationId, entity.Amount, entity.Currency, entity.Id, cancellationToken);
    }

    protected override async Task AfterSaveAsync(PaymentEntity entity, CancellationToken cancellationToken)
    {
        var reservation = await UnitOfWork.Reservations.Query()
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == entity.ReservationId, cancellationToken);

        if (reservation is null)
        {
            return;
        }

        var changed = false;
        if (entity.PaymentStatus is PaymentStatus.Completed && reservation.Status != ReservationStatus.Confirmed)
        {
            reservation.Status = ReservationStatus.Confirmed;
            reservation.ExpiresAt = null;
            changed = true;
        }

        if (entity.PaymentStatus is PaymentStatus.Refunded)
        {
            if (reservation.Status != ReservationStatus.Cancelled)
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.ExpiresAt = null;
                changed = true;
            }

            foreach (var ticket in reservation.Tickets.Where(x => x.TicketStatus is TicketStatus.Issued or TicketStatus.CheckedIn))
            {
                ticket.TicketStatus = TicketStatus.Refunded;
                changed = true;
            }
        }

        if (!changed)
        {
            return;
        }

        UnitOfWork.Reservations.Update(reservation);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    protected override PaymentEntity MapCreateRequest(PaymentCreateRequest request) => new()
    {
        ReservationId = request.ReservationId,
        Amount = request.Amount,
        Currency = request.Currency.Trim().ToUpperInvariant(),
        PaymentMethod = ParseEnum<PaymentMethod>(request.PaymentMethod),
        PaymentStatus = ParseEnum<PaymentStatus>(request.PaymentStatus),
        TransactionReference = request.TransactionReference,
        PaidAt = request.PaidAt
    };

    protected override void ApplyUpdateRequest(PaymentEntity entity, PaymentUpdateRequest request)
    {
        entity.ReservationId = request.ReservationId;
        entity.Amount = request.Amount;
        entity.Currency = request.Currency.Trim().ToUpperInvariant();
        entity.PaymentMethod = ParseEnum<PaymentMethod>(request.PaymentMethod);
        entity.PaymentStatus = ParseEnum<PaymentStatus>(request.PaymentStatus);
        entity.TransactionReference = request.TransactionReference;
        entity.PaidAt = request.PaidAt;
    }

    protected override PaymentResponse MapToResponse(PaymentEntity entity) => new()
    {
        Id = entity.Id,
        ReservationId = entity.ReservationId,
        Amount = entity.Amount,
        Currency = entity.Currency,
        PaymentMethod = entity.PaymentMethod.ToString(),
        PaymentStatus = entity.PaymentStatus.ToString(),
        TransactionReference = entity.TransactionReference,
        PaidAt = entity.PaidAt,
        CreatedAt = entity.CreatedAt
    };

    private async Task ValidateReservationPaymentAsync(int reservationId, decimal amount, string currency, int? currentPaymentId, CancellationToken cancellationToken)
    {
        var reservation = await UnitOfWork.Reservations.Query()
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);

        if (reservation is null)
        {
            throw new InvalidOperationException("Reservation was not found.");
        }

        if (!currency.Equals(reservation.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Payment currency must be {reservation.Currency}.");
        }

        if (Math.Round(amount, 2) != Math.Round(reservation.TotalPrice, 2))
        {
            throw new InvalidOperationException($"Payment amount must match the reservation total: {reservation.TotalPrice:0.00} {reservation.Currency}.");
        }

        var hasCompletedPayment = await UnitOfWork.Payments.Query()
            .AnyAsync(x =>
                x.ReservationId == reservationId &&
                x.PaymentStatus == PaymentStatus.Completed &&
                (!currentPaymentId.HasValue || x.Id != currentPaymentId.Value),
                cancellationToken);

        if (hasCompletedPayment)
        {
            throw new InvalidOperationException("Reservation already has a completed payment.");
        }
    }
}
