using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Controllers;

public class FlightsController : Controller
{
    private readonly AirlineApiClient _apiClient;

    public FlightsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(FlightSearchViewModel model, CancellationToken cancellationToken)
    {
        if (!this.IsSignedIn())
        {
            return this.RedirectToLogin();
        }

        model.PageNumber = Math.Max(1, model.PageNumber);
        model.PageSize = model.PageSize <= 0 ? 10 : Math.Clamp(model.PageSize, 1, 50);
        model.SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? "departureTime" : model.SortBy;
        model.SortDirection = model.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

        try
        {
            await LoadAirportsAsync(model, cancellationToken);

            var path = QueryStringBuilder.Build(
                "flights",
                ("flightNumber", model.FlightNumber),
                ("departureAirportId", model.DepartureAirportId),
                ("arrivalAirportId", model.ArrivalAirportId),
                ("departureDate", model.DepartureDate?.ToString("yyyy-MM-dd")),
                ("status", model.Status),
                ("minPrice", model.MinPrice),
                ("maxPrice", model.MaxPrice),
                ("pageNumber", model.PageNumber),
                ("pageSize", model.PageSize),
                ("sortBy", model.SortBy),
                ("sortDirection", model.SortDirection));

            model.Results = await _apiClient.GetAsync<PagedResult<FlightResponse>>(path, cancellationToken);
            return View(model);
        }
        catch (ApiException)
        {
            ModelState.AddModelError(string.Empty, "Could not load flights. Please sign in again or check the API.");
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API is not reachable. Start the API project and try again.");
            return View(model);
        }
    }

    private async Task LoadAirportsAsync(FlightSearchViewModel model, CancellationToken cancellationToken)
    {
        var airports = await _apiClient.GetAsync<PagedResult<AirportResponse>>(
            "airports?pageNumber=1&pageSize=100&sortBy=city&sortDirection=asc&isActive=true",
            cancellationToken);

        model.Airports = airports.Items;
    }
}
