using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public DashboardController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await CountAsync<UserResponse>("users", cancellationToken);
        var airports = await CountAsync<AirportResponse>("airports", cancellationToken);
        var aircraft = await CountAsync<AircraftResponse>("aircraft", cancellationToken);
        var flights = await CountAsync<FlightResponse>("flights", cancellationToken);
        var reservations = await CountAsync<ReservationResponse>("reservations", cancellationToken);
        var tickets = await CountAsync<TicketResponse>("tickets", cancellationToken);
        var payments = await CountAsync<PaymentResponse>("payments", cancellationToken);

        return View(new AdminDashboardViewModel
        {
            UsersCount = users,
            AirportsCount = airports,
            AircraftCount = aircraft,
            FlightsCount = flights,
            ReservationsCount = reservations,
            TicketsCount = tickets,
            PaymentsCount = payments
        });
    }

    private async Task<int> CountAsync<T>(string path, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetAsync<PagedResult<T>>($"{path}?pageNumber=1&pageSize=1", cancellationToken);
        return result.TotalItems;
    }
}
