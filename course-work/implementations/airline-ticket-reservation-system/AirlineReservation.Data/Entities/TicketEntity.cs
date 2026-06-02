using System.ComponentModel.DataAnnotations;
using AirlineReservation.Data.Enums;

namespace AirlineReservation.Data.Entities;

public class TicketEntity : BaseEntity
{
    public int ReservationId { get; set; }

    public ReservationEntity? Reservation { get; set; }

    [Required]
    [MaxLength(50)]
    public string PassengerFirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string PassengerLastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string PassengerDocumentNumber { get; set; } = string.Empty;

    public PassengerType PassengerType { get; set; } = PassengerType.Adult;

    [MaxLength(5)]
    public string? SeatNumber { get; set; }

    public FareClass FareClass { get; set; } = FareClass.Economy;

    public decimal Price { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    public TicketStatus TicketStatus { get; set; } = TicketStatus.Issued;

    public DateTime? IssuedAt { get; set; }
}
