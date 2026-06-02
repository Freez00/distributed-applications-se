using System.ComponentModel.DataAnnotations;
using AirlineReservation.Data.Enums;

namespace AirlineReservation.Data.Entities;

public class FlightEntity : BaseEntity
{
    [Required]
    [MaxLength(10)]
    public string FlightNumber { get; set; } = string.Empty;

    public int DepartureAirportId { get; set; }

    public AirportEntity? DepartureAirport { get; set; }

    public int ArrivalAirportId { get; set; }

    public AirportEntity? ArrivalAirport { get; set; }

    public int AircraftId { get; set; }

    public AircraftEntity? Aircraft { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public decimal BasePrice { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

    public ICollection<ReservationEntity> Reservations { get; set; } = new List<ReservationEntity>();
}
