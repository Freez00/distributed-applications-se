using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class AircraftController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public AircraftController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? registrationNumber, string? model, string? manufacturer, bool? isActive, CancellationToken cancellationToken)
    {
        var path = QueryStringBuilder.Build(
            "aircraft",
            ("registrationNumber", registrationNumber),
            ("model", model),
            ("manufacturer", manufacturer),
            ("isActive", isActive),
            ("pageSize", 100),
            ("sortBy", "registrationNumber"));

        ViewBag.RegistrationNumber = registrationNumber;
        ViewBag.Model = model;
        ViewBag.Manufacturer = manufacturer;
        ViewBag.IsActive = isActive;
        return View(await _apiClient.GetAsync<PagedResult<AircraftResponse>>(path, cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        => View(await _apiClient.GetAsync<AircraftResponse>($"aircraft/{id}", cancellationToken));

    public IActionResult Create()
        => View(new AircraftCreateRequest { IsActive = true, ManufactureYear = DateTime.Now.Year });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AircraftCreateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            await _apiClient.PostAsync<AircraftResponse>("aircraft", request, cancellationToken: cancellationToken);
            TempData["Success"] = "Aircraft was created.";
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
        var aircraft = await _apiClient.GetAsync<AircraftResponse>($"aircraft/{id}", cancellationToken);
        return View(new AircraftUpdateRequest
        {
            RegistrationNumber = aircraft.RegistrationNumber,
            Model = aircraft.Model,
            Manufacturer = aircraft.Manufacturer,
            SeatCapacity = aircraft.SeatCapacity,
            RangeKm = aircraft.RangeKm,
            ManufactureYear = aircraft.ManufactureYear,
            IsActive = aircraft.IsActive,
            LastMaintenanceDate = aircraft.LastMaintenanceDate
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AircraftUpdateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            await _apiClient.PutAsync<AircraftResponse>($"aircraft/{id}", request, cancellationToken);
            TempData["Success"] = "Aircraft was updated.";
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
            await _apiClient.DeleteAsync($"aircraft/{id}", cancellationToken);
            TempData["Success"] = "Aircraft was deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Aircraft could not be deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"aircraft/{id}/force", cancellationToken);
            TempData["Success"] = "Aircraft and related flights/reservations were force deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Aircraft could not be force deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }
}
