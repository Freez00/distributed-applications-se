using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class FlightsController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public FlightsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? flightNumber, int? departureAirportId, int? arrivalAirportId, string? status, CancellationToken cancellationToken)
    {
        var path = QueryStringBuilder.Build(
            "flights",
            ("flightNumber", flightNumber),
            ("departureAirportId", departureAirportId),
            ("arrivalAirportId", arrivalAirportId),
            ("status", status),
            ("pageSize", 100),
            ("sortBy", "departureTime"));

        ViewBag.FlightNumber = flightNumber;
        ViewBag.DepartureAirportId = departureAirportId;
        ViewBag.ArrivalAirportId = arrivalAirportId;
        ViewBag.Status = status;
        ViewBag.Airports = await LoadAirportsAsync(cancellationToken);
        ViewBag.Aircraft = await LoadAircraftAsync(cancellationToken);

        return View(await _apiClient.GetAsync<PagedResult<FlightResponse>>(path, cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var flight = await _apiClient.GetAsync<FlightResponse>($"flights/{id}", cancellationToken);
        var airports = await LoadAirportsAsync(cancellationToken);
        var aircraft = await LoadAircraftAsync(cancellationToken);

        return View(new AdminFlightDetailsViewModel
        {
            Flight = flight,
            DepartureAirport = airports.FirstOrDefault(x => x.Id == flight.DepartureAirportId),
            ArrivalAirport = airports.FirstOrDefault(x => x.Id == flight.ArrivalAirportId),
            Aircraft = aircraft.FirstOrDefault(x => x.Id == flight.AircraftId)
        });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
        => View(await BuildFlightFormAsync(new AdminFlightFormViewModel(), cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminFlightFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFlightFormAsync(model, cancellationToken));
        }

        try
        {
            await _apiClient.PostAsync<FlightResponse>("flights", model.Flight, cancellationToken: cancellationToken);
            TempData["Success"] = "Flight was created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(await BuildFlightFormAsync(model, cancellationToken));
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var flight = await _apiClient.GetAsync<FlightResponse>($"flights/{id}", cancellationToken);
        return View(await BuildFlightFormAsync(new AdminFlightFormViewModel
        {
            Id = id,
            Flight = new FlightCreateRequest
            {
                FlightNumber = flight.FlightNumber,
                DepartureAirportId = flight.DepartureAirportId,
                ArrivalAirportId = flight.ArrivalAirportId,
                AircraftId = flight.AircraftId,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BasePrice = flight.BasePrice,
                Currency = flight.Currency,
                Status = flight.Status
            }
        }, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminFlightFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(await BuildFlightFormAsync(model, cancellationToken));
        }

        try
        {
            await _apiClient.PutAsync<FlightResponse>($"flights/{id}", model.Flight, cancellationToken);
            TempData["Success"] = "Flight was updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(await BuildFlightFormAsync(model, cancellationToken));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"flights/{id}", cancellationToken);
            TempData["Success"] = "Flight was deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Flight could not be deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"flights/{id}/force", cancellationToken);
            TempData["Success"] = "Flight and related reservations were force deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Flight could not be force deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminFlightFormViewModel> BuildFlightFormAsync(AdminFlightFormViewModel model, CancellationToken cancellationToken)
    {
        model.Airports = await LoadAirportsAsync(cancellationToken);
        model.Aircraft = await LoadAircraftAsync(cancellationToken);
        return model;
    }

    private async Task<IReadOnlyCollection<AirportResponse>> LoadAirportsAsync(CancellationToken cancellationToken)
    {
        var airports = await _apiClient.GetAsync<PagedResult<AirportResponse>>("airports?pageSize=100&sortBy=code", cancellationToken);
        return airports.Items;
    }

    private async Task<IReadOnlyCollection<AircraftResponse>> LoadAircraftAsync(CancellationToken cancellationToken)
    {
        var aircraft = await _apiClient.GetAsync<PagedResult<AircraftResponse>>("aircraft?pageSize=100&sortBy=registrationNumber", cancellationToken);
        return aircraft.Items;
    }
}
