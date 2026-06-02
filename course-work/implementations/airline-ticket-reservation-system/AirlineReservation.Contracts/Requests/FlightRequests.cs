using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;

namespace AirlineReservation.Contracts.Requests;

public class FlightFilterRequest : PagedRequest
{
    [MaxLength(10, ErrorMessage = ValidationMessages.MaxLength)]
    public string? FlightNumber { get; set; }

    public int? DepartureAirportId { get; set; }

    public int? ArrivalAirportId { get; set; }

    public DateTime? DepartureDate { get; set; }

    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Status { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public decimal? MaxPrice { get; set; }
}

public class FlightCreateRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(10, ErrorMessage = ValidationMessages.MaxLength)]
    public string FlightNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public int DepartureAirportId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public int ArrivalAirportId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.Range)]
    public int AircraftId { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = ValidationMessages.Range)]
    public decimal BasePrice { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(3, ErrorMessage = ValidationMessages.MaxLength)]
    public string Currency { get; set; } = "EUR";

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string Status { get; set; } = "Scheduled";
}

public class FlightUpdateRequest : FlightCreateRequest;
