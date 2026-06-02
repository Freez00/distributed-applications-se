namespace AirlineReservation.Contracts.Responses;

public class FlightResponse
{
    public int Id { get; set; }

    public string FlightNumber { get; set; } = string.Empty;

    public int DepartureAirportId { get; set; }

    public int ArrivalAirportId { get; set; }

    public int AircraftId { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public decimal BasePrice { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int AvailableSeats { get; set; }

    public DateTime CreatedAt { get; set; }
}
