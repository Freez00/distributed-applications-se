using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;

namespace AirlineReservation.Contracts.Requests;

public class ReservationFilterRequest : PagedRequest
{
    public int? UserId { get; set; }

    public int? FlightId { get; set; }

    [MaxLength(12, ErrorMessage = ValidationMessages.MaxLength)]
    public string? ReservationCode { get; set; }

    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Status { get; set; }

    public DateTime? BookingDateFrom { get; set; }

    public DateTime? BookingDateTo { get; set; }
}

public class ReservationCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public int FlightId { get; set; }

    [Required(ErrorMessage = "At least one passenger ticket is required.")]
    [MinLength(1, ErrorMessage = "At least one passenger ticket is required.")]
    [MaxLength(9, ErrorMessage = "A reservation can contain at most 9 passenger tickets.")]
    public List<ReservationTicketDraftRequest> Tickets { get; set; } = [];
}

public class ReservationUpdateRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string Status { get; set; } = "Pending";

    public DateTime? ExpiresAt { get; set; }
}

public class ReservationTicketDraftRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string PassengerFirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string PassengerLastName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(5, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string PassengerDocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string PassengerType { get; set; } = "Adult";

    [MaxLength(5, ErrorMessage = ValidationMessages.MaxLength)]
    public string? SeatNumber { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string FareClass { get; set; } = "Economy";
}
