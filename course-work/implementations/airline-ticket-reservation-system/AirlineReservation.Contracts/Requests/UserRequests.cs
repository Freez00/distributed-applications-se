using System.ComponentModel.DataAnnotations;
using AirlineReservation.Contracts.Common;

namespace AirlineReservation.Contracts.Requests;

public class UserFilterRequest : PagedRequest
{
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string? FirstName { get; set; }

    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string? LastName { get; set; }

    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Email { get; set; }

    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}

public class UserCreateRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [EmailAddress(ErrorMessage = ValidationMessages.Email)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(8, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string Role { get; set; } = "Customer";

    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UserUpdateRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [EmailAddress(ErrorMessage = ValidationMessages.Email)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string Role { get; set; } = "Customer";

    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
}
