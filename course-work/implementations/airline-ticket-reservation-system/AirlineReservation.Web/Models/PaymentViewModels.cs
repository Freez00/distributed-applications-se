using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Responses;

namespace AirlineReservation.Web.Models;

public class CreatePaymentViewModel
{
    public int ReservationId { get; set; }

    public ReservationResponse? Reservation { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = ValidationMessages.Range)]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(3, ErrorMessage = ValidationMessages.MaxLength)]
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "EUR";

    [Required(ErrorMessage = ValidationMessages.Required)]
    [Display(Name = "Payment method")]
    public string PaymentMethod { get; set; } = "Card";

    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    [Display(Name = "Transaction reference")]
    public string? TransactionReference { get; set; }
}
