using System.ComponentModel.DataAnnotations;

namespace AirlineReservation.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [EmailAddress(ErrorMessage = ValidationMessages.Email)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(8, ErrorMessage = ValidationMessages.MinLength)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(2, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [EmailAddress(ErrorMessage = ValidationMessages.Email)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationMessages.Required)]
    [MinLength(8, ErrorMessage = ValidationMessages.MinLength)]
    [MaxLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = ValidationMessages.MaxLength)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }
}
