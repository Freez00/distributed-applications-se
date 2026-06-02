using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;

namespace AirlineReservation.Contracts.Requests;

public class TicketFilterRequest : PagedRequest
{
    public int? ReservationId { get; set; }

    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PassengerLastName { get; set; }

    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PassengerDocumentNumber { get; set; }

    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string? FareClass { get; set; }

    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string? TicketStatus { get; set; }
}

public class TicketCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public int ReservationId { get; set; }

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

    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = ValidationMessages.Range)]
    public decimal Price { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(3, ErrorMessage = ValidationMessages.MaxLength)]
    public string Currency { get; set; } = "EUR";

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string TicketStatus { get; set; } = "Issued";

    public DateTime? IssuedAt { get; set; }
}

public class TicketUpdateRequest : TicketCreateRequest;
