using System.ComponentModel.DataAnnotations;

namespace AirlineReservation.Data.Entities;

public class AircraftEntity : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Manufacturer { get; set; } = string.Empty;

    public int SeatCapacity { get; set; }

    public int RangeKm { get; set; }

    public int ManufactureYear { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastMaintenanceDate { get; set; }

    public ICollection<FlightEntity> Flights { get; set; } = new List<FlightEntity>();
}
