using System.ComponentModel.DataAnnotations;
using AirlineReservation.Data.Enums;

namespace AirlineReservation.Data.Entities;

public class ReservationEntity : BaseEntity
{
    public int UserId { get; set; }

    public UserEntity? User { get; set; }

    public int FlightId { get; set; }

    public FlightEntity? Flight { get; set; }

    [Required]
    [MaxLength(12)]
    public string ReservationCode { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public decimal TotalPrice { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public int PassengerCount { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();

    public ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
}
