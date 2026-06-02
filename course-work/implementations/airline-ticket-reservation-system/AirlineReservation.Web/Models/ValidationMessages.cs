namespace AirlineReservation.Web.Models;

public static class ValidationMessages
{
    public const string Required = "{0} is required.";

    public const string MinLength = "{0} must be at least {1} characters.";

    public const string MaxLength = "{0} must be at most {1} characters.";

    public const string Range = "{0} must be between {1} and {2}.";

    public const string Email = "{0} must be a valid email address.";
}
