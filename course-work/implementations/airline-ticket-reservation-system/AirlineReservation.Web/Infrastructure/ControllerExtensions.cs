using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Infrastructure;

public static class ControllerExtensions
{
    public static bool IsSignedIn(this Controller controller)
        => !string.IsNullOrWhiteSpace(controller.HttpContext.Session.GetString(SessionKeys.AuthToken));

    public static int? CurrentUserId(this Controller controller)
    {
        var value = controller.HttpContext.Session.GetString(SessionKeys.UserId);
        return int.TryParse(value, out var id) ? id : null;
    }

    public static IActionResult RedirectToLogin(this Controller controller)
        => controller.RedirectToAction(
            "Login",
            "Account",
            new { returnUrl = controller.HttpContext.Request.Path + controller.HttpContext.Request.QueryString });
}
