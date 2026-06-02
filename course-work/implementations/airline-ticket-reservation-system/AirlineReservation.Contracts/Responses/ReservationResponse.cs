namespace AirlineReservation.Contracts.Responses;

public class ReservationResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int FlightId { get; set; }

    public string ReservationCode { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int PassengerCount { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
