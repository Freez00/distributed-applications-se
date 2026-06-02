using AirlineReservation.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

[Area("Admin")]
public abstract class AdminControllerBase : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var token = HttpContext.Session.GetString(SessionKeys.AuthToken);
        var role = HttpContext.Session.GetString(SessionKeys.UserRole);

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = RedirectToAction("Login", "Account", new { area = "", returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
            return;
        }

        if (role != "Admin")
        {
            context.Result = RedirectToAction("Index", "Home", new { area = "" });
            return;
        }

        base.OnActionExecuting(context);
    }

    protected void AddApiError(ApiException exception)
    {
        foreach (var message in exception.FriendlyMessages)
        {
            ModelState.AddModelError(string.Empty, message);
        }
    }
}
