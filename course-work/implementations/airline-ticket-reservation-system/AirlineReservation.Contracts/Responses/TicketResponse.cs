namespace AirlineReservation.Contracts.Responses;

public class TicketResponse
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public string PassengerFirstName { get; set; } = string.Empty;

    public string PassengerLastName { get; set; } = string.Empty;

    public string PassengerDocumentNumber { get; set; } = string.Empty;

    public string PassengerType { get; set; } = string.Empty;

    public string? SeatNumber { get; set; }

    public string FareClass { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string TicketStatus { get; set; } = string.Empty;

    public DateTime? IssuedAt { get; set; }
}
