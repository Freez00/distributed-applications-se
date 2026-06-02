using System.ComponentModel.DataAnnotations;

namespace AirlineReservation.Contracts.Requests;

public class RegisterRequest
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

    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [EmailAddress(ErrorMessage = ValidationMessages.Email)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(8, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string Password { get; set; } = string.Empty;
}
