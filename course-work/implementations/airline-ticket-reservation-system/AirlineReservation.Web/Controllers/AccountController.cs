using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Controllers;

public class AccountController : Controller
{
    private readonly AirlineApiClient _apiClient;

    public AccountController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (this.IsSignedIn())
        {
            return RedirectToAction("Index", "Flights");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var auth = await _apiClient.PostAsync<AuthResponse>(
                "auth/login",
                new LoginRequest { Email = model.Email, Password = model.Password },
                authorize: false,
                cancellationToken);

            StoreAuthSession(auth);
            return RedirectAfterAuth(model.ReturnUrl);
        }
        catch (ApiException)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API is not reachable. Start the API project and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (this.IsSignedIn())
        {
            return RedirectToAction("Index", "Flights");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var auth = await _apiClient.PostAsync<AuthResponse>(
                "auth/register",
                new RegisterRequest
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    PhoneNumber = model.PhoneNumber
                },
                authorize: false,
                cancellationToken);

            StoreAuthSession(auth);
            return RedirectToAction("Index", "Flights");
        }
        catch (ApiException)
        {
            ModelState.AddModelError(string.Empty, "Registration failed. The email may already be used.");
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API is not reachable. Start the API project and try again.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    private void StoreAuthSession(AuthResponse auth)
    {
        HttpContext.Session.SetString(SessionKeys.AuthToken, auth.Token);
        HttpContext.Session.SetString(SessionKeys.UserId, auth.User.Id.ToString());
        HttpContext.Session.SetString(SessionKeys.UserName, $"{auth.User.FirstName} {auth.User.LastName}");
        HttpContext.Session.SetString(SessionKeys.UserEmail, auth.User.Email);
        HttpContext.Session.SetString(SessionKeys.UserRole, auth.User.Role);
    }

    private IActionResult RedirectAfterAuth(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Flights");
    }
}
