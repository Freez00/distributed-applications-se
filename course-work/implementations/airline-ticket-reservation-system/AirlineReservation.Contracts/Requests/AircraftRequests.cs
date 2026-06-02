using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;

namespace AirlineReservation.Contracts.Requests;

public class AircraftFilterRequest : PagedRequest
{
    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Model { get; set; }

    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Manufacturer { get; set; }

    public bool? IsActive { get; set; }
}

public class AircraftCreateRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string Manufacturer { get; set; } = string.Empty;

    [Range(1, 900, ErrorMessage = ValidationMessages.Range)]
    public int SeatCapacity { get; set; }

    [Range(1, 20000, ErrorMessage = ValidationMessages.Range)]
    public int RangeKm { get; set; }

    [Range(1950, 2100, ErrorMessage = ValidationMessages.Range)]
    public int ManufactureYear { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastMaintenanceDate { get; set; }
}

public class AircraftUpdateRequest : AircraftCreateRequest;
