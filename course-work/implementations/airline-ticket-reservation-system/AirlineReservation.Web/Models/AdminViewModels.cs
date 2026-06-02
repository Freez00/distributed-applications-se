using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;

namespace AirlineReservation.Web.Models;

public class AdminDashboardViewModel
{
    public int UsersCount { get; set; }

    public int AirportsCount { get; set; }

    public int AircraftCount { get; set; }

    public int FlightsCount { get; set; }

    public int ReservationsCount { get; set; }

    public int TicketsCount { get; set; }

    public int PaymentsCount { get; set; }
}

public class AdminFlightFormViewModel
{
    public int? Id { get; set; }

    public FlightCreateRequest Flight { get; set; } = new()
    {
        Currency = "EUR",
        Status = "Scheduled",
        DepartureTime = DateTime.Now.AddDays(1),
        ArrivalTime = DateTime.Now.AddDays(1).AddHours(2)
    };

    public IReadOnlyCollection<AirportResponse> Airports { get; set; } = [];

    public IReadOnlyCollection<AircraftResponse> Aircraft { get; set; } = [];
}

public class AdminFlightDetailsViewModel
{
    public FlightResponse Flight { get; set; } = new();

    public AirportResponse? DepartureAirport { get; set; }

    public AirportResponse? ArrivalAirport { get; set; }

    public AircraftResponse? Aircraft { get; set; }
}

public class AdminReservationDetailsViewModel
{
    public ReservationResponse Reservation { get; set; } = new();

    public UserResponse? User { get; set; }

    public FlightResponse? Flight { get; set; }

    public AirportResponse? DepartureAirport { get; set; }

    public AirportResponse? ArrivalAirport { get; set; }

    public IReadOnlyCollection<TicketResponse> Tickets { get; set; } = [];

    public IReadOnlyCollection<PaymentResponse> Payments { get; set; } = [];
}

public class AdminReservationEditViewModel
{
    public int Id { get; set; }

    public string ReservationCode { get; set; } = string.Empty;

    public ReservationUpdateRequest Reservation { get; set; } = new();
}

public class AdminTicketDetailsViewModel
{
    public TicketResponse Ticket { get; set; } = new();

    public ReservationResponse? Reservation { get; set; }
}

public class AdminTicketFormViewModel
{
    public int? Id { get; set; }

    public TicketCreateRequest Ticket { get; set; } = new()
    {
        PassengerType = "Adult",
        FareClass = "Economy",
        Currency = "EUR",
        TicketStatus = "Issued",
        IssuedAt = DateTime.UtcNow
    };

    public IReadOnlyCollection<ReservationResponse> Reservations { get; set; } = [];
}

public class AdminPaymentDetailsViewModel
{
    public PaymentResponse Payment { get; set; } = new();

    public ReservationResponse? Reservation { get; set; }
}

public class AdminPaymentFormViewModel
{
    public int? Id { get; set; }

    public PaymentCreateRequest Payment { get; set; } = new()
    {
        Currency = "EUR",
        PaymentMethod = "Card",
        PaymentStatus = "Pending"
    };

    public IReadOnlyCollection<ReservationResponse> Reservations { get; set; } = [];
}

public class AdminReservationIndexViewModel
{
    public string? ReservationCode { get; set; }

    public string? Status { get; set; }

    public int? UserId { get; set; }

    public int? FlightId { get; set; }

    public PagedResult<ReservationResponse> Results { get; set; } = new();

    public IReadOnlyCollection<UserResponse> Users { get; set; } = [];

    public IReadOnlyCollection<FlightResponse> Flights { get; set; } = [];
}

public class AdminTicketIndexViewModel
{
    public int? ReservationId { get; set; }

    public string? PassengerLastName { get; set; }

    public string? TicketStatus { get; set; }

    public PagedResult<TicketResponse> Results { get; set; } = new();

    public IReadOnlyCollection<ReservationResponse> Reservations { get; set; } = [];
}

public class AdminPaymentIndexViewModel
{
    public int? ReservationId { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionReference { get; set; }

    public PagedResult<PaymentResponse> Results { get; set; } = new();

    public IReadOnlyCollection<ReservationResponse> Reservations { get; set; } = [];
}
