using System.ComponentModel.DataAnnotations;
using AirlineReservation.Data.Enums;

namespace AirlineReservation.Data.Entities;

public class PaymentEntity : BaseEntity
{
    public int ReservationId { get; set; }

    public ReservationEntity? Reservation { get; set; }

    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [MaxLength(100)]
    public string? TransactionReference { get; set; }

    public DateTime? PaidAt { get; set; }
}
