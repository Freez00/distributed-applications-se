using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Responses;

namespace AirlineReservation.Web.Models;

public class FlightSearchViewModel
{
    [Display(Name = "Flight number")]
    public string? FlightNumber { get; set; }

    [Display(Name = "From")]
    public int? DepartureAirportId { get; set; }

    [Display(Name = "To")]
    public int? ArrivalAirportId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Departure date")]
    public DateTime? DepartureDate { get; set; }

    [Display(Name = "Status")]
    public string? Status { get; set; }

    [Range(0, 999999, ErrorMessage = ValidationMessages.Range)]
    [Display(Name = "Min price")]
    public decimal? MinPrice { get; set; }

    [Range(0, 999999, ErrorMessage = ValidationMessages.Range)]
    [Display(Name = "Max price")]
    public decimal? MaxPrice { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string SortBy { get; set; } = "departureTime";

    public string SortDirection { get; set; } = "asc";

    public IReadOnlyCollection<AirportResponse> Airports { get; set; } = [];

    public PagedResult<FlightResponse>? Results { get; set; }

    public string AirportLabel(int airportId)
    {
        var airport = Airports.FirstOrDefault(x => x.Id == airportId);
        return airport is null ? $"Airport #{airportId}" : $"{airport.Code} - {airport.City}";
    }
}
