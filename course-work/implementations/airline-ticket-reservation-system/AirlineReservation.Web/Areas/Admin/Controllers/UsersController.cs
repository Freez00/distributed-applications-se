using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class UsersController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public UsersController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? email, string? role, bool? isActive, CancellationToken cancellationToken)
    {
        var path = QueryStringBuilder.Build(
            "users",
            ("email", email),
            ("role", role),
            ("isActive", isActive),
            ("pageSize", 100),
            ("sortBy", "id"));

        ViewBag.Email = email;
        ViewBag.Role = role;
        ViewBag.IsActive = isActive;
        return View(await _apiClient.GetAsync<PagedResult<UserResponse>>(path, cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        => View(await _apiClient.GetAsync<UserResponse>($"users/{id}", cancellationToken));

    public IActionResult Create()
        => View(new UserCreateRequest { Role = "Customer", IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            await _apiClient.PostAsync<UserResponse>("users", request, cancellationToken: cancellationToken);
            TempData["Success"] = "User was created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(request);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var user = await _apiClient.GetAsync<UserResponse>($"users/{id}", cancellationToken);
        return View(new UserUpdateRequest
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserUpdateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            await _apiClient.PutAsync<UserResponse>($"users/{id}", request, cancellationToken);
            TempData["Success"] = "User was updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"users/{id}", cancellationToken);
            TempData["Success"] = "User was deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"User could not be deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"users/{id}/force", cancellationToken);
            TempData["Success"] = "User and related reservations were force deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"User could not be force deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }
}
