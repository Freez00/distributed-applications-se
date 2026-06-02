using System.ComponentModel.DataAnnotations;

namespace AirlineReservation.Data.Entities;

public class AirportEntity : BaseEntity
{
    [Required]
    [MaxLength(3)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Timezone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<FlightEntity> DepartingFlights { get; set; } = new List<FlightEntity>();

    public ICollection<FlightEntity> ArrivingFlights { get; set; } = new List<FlightEntity>();
}
