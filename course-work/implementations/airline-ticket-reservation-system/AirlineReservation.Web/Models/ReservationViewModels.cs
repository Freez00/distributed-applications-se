using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Responses;

namespace AirlineReservation.Web.Models;

public class ReservationListViewModel
{
    public IReadOnlyCollection<ReservationResponse> Reservations { get; set; } = [];

    public IReadOnlyCollection<FlightResponse> Flights { get; set; } = [];
}

public class ReservationDetailsViewModel
{
    public ReservationResponse Reservation { get; set; } = new();

    public FlightResponse? Flight { get; set; }

    public AirportResponse? DepartureAirport { get; set; }

    public AirportResponse? ArrivalAirport { get; set; }

    public IReadOnlyCollection<TicketResponse> Tickets { get; set; } = [];

    public IReadOnlyCollection<PaymentResponse> Payments { get; set; } = [];

    public PaymentResponse? CompletedPayment
        => Payments.FirstOrDefault(x => x.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase));

    public bool HasCompletedPayment
        => CompletedPayment is not null;

    public bool CanRecordPayment
        => Reservation.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) && !HasCompletedPayment;
}

public class CreateReservationViewModel
{
    public const int MaxPassengerCount = 9;

    public int FlightId { get; set; }

    public FlightResponse? Flight { get; set; }

    [Range(1, MaxPassengerCount, ErrorMessage = ValidationMessages.Range)]
    [Display(Name = "Passengers")]
    public int PassengerCount { get; set; } = 1;

    public List<CreateTicketDraftViewModel> Tickets { get; set; } = CreateTicketSlots();

    public void EnsureTicketSlots()
    {
        PassengerCount = Math.Clamp(PassengerCount, 1, MaxPassengerCount);

        while (Tickets.Count < MaxPassengerCount)
        {
            Tickets.Add(new CreateTicketDraftViewModel());
        }
    }

    private static List<CreateTicketDraftViewModel> CreateTicketSlots()
        => Enumerable.Range(0, MaxPassengerCount).Select(_ => new CreateTicketDraftViewModel()).ToList();
}

public class CreateTicketDraftViewModel
{
    [Display(Name = "First name")]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PassengerFirstName { get; set; }

    [Display(Name = "Last name")]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PassengerLastName { get; set; }

    [Display(Name = "Document number")]
    [MinLength(5, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PassengerDocumentNumber { get; set; }

    [Display(Name = "Passenger type")]
    public string PassengerType { get; set; } = "Adult";

    [Display(Name = "Seat")]
    [MaxLength(5, ErrorMessage = ValidationMessages.MaxLength)]
    public string? SeatNumber { get; set; }

    [Display(Name = "Fare class")]
    public string FareClass { get; set; } = "Economy";

    public bool HasAnyValue()
        => !string.IsNullOrWhiteSpace(PassengerFirstName)
            || !string.IsNullOrWhiteSpace(PassengerLastName)
            || !string.IsNullOrWhiteSpace(PassengerDocumentNumber)
            || !string.IsNullOrWhiteSpace(SeatNumber);
}
