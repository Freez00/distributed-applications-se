using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;

namespace AirlineReservation.Contracts.Requests;

public class PaymentFilterRequest : PagedRequest
{
    public int? ReservationId { get; set; }

    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PaymentStatus { get; set; }

    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PaymentMethod { get; set; }

    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string? TransactionReference { get; set; }
}

public class PaymentCreateRequest
{
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public int ReservationId { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = ValidationMessages.Range)]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(3, ErrorMessage = ValidationMessages.MaxLength)]
    public string Currency { get; set; } = "EUR";

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string PaymentMethod { get; set; } = "Card";

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string PaymentStatus { get; set; } = "Pending";

    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string? TransactionReference { get; set; }

    public DateTime? PaidAt { get; set; }
}

public class PaymentUpdateRequest : PaymentCreateRequest;
