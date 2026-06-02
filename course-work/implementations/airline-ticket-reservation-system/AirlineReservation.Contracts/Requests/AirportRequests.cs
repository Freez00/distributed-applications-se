using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;

namespace AirlineReservation.Contracts.Requests;

public class AirportFilterRequest : PagedRequest
{
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(3, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Code { get; set; }

    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string? City { get; set; }

    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Country { get; set; }

    public bool? IsActive { get; set; }
}

public class AirportCreateRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(3, ErrorMessage = ValidationMessages.MaxLength)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(3, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string Timezone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class AirportUpdateRequest : AirportCreateRequest;
