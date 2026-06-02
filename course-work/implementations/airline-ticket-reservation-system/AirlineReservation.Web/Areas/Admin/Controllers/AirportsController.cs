using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class AirportsController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public AirportsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? code, string? city, string? country, bool? isActive, CancellationToken cancellationToken)
    {
        var path = QueryStringBuilder.Build(
            "airports",
            ("code", code),
            ("city", city),
            ("country", country),
            ("isActive", isActive),
            ("pageSize", 100),
            ("sortBy", "code"));

        ViewBag.Code = code;
        ViewBag.City = city;
        ViewBag.Country = country;
        ViewBag.IsActive = isActive;
        return View(await _apiClient.GetAsync<PagedResult<AirportResponse>>(path, cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        => View(await _apiClient.GetAsync<AirportResponse>($"airports/{id}", cancellationToken));

    public IActionResult Create()
        => View(new AirportCreateRequest { IsActive = true, Timezone = "Europe/Sofia" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AirportCreateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            await _apiClient.PostAsync<AirportResponse>("airports", request, cancellationToken: cancellationToken);
            TempData["Success"] = "Airport was created.";
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
        var airport = await _apiClient.GetAsync<AirportResponse>($"airports/{id}", cancellationToken);
        return View(new AirportUpdateRequest
        {
            Code = airport.Code,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country,
            Timezone = airport.Timezone,
            IsActive = airport.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AirportUpdateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            await _apiClient.PutAsync<AirportResponse>($"airports/{id}", request, cancellationToken);
            TempData["Success"] = "Airport was updated.";
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
            await _apiClient.DeleteAsync($"airports/{id}", cancellationToken);
            TempData["Success"] = "Airport was deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Airport could not be deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"airports/{id}/force", cancellationToken);
            TempData["Success"] = "Airport and related flights/reservations were force deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Airport could not be force deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }
}
