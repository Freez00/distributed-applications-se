using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class ReservationsController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public ReservationsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(AdminReservationIndexViewModel model, CancellationToken cancellationToken)
    {
        var path = QueryStringBuilder.Build(
            "reservations",
            ("reservationCode", model.ReservationCode),
            ("status", model.Status),
            ("userId", model.UserId),
            ("flightId", model.FlightId),
            ("pageSize", 100),
            ("sortBy", "bookingDate"),
            ("sortDirection", "desc"));

        model.Results = await _apiClient.GetAsync<PagedResult<ReservationResponse>>(path, cancellationToken);
        model.Users = await LoadUsersAsync(cancellationToken);
        model.Flights = await LoadFlightsAsync(cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        => View(await BuildDetailsAsync(id, cancellationToken));

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{id}", cancellationToken);
        return View(new AdminReservationEditViewModel
        {
            Id = reservation.Id,
            ReservationCode = reservation.ReservationCode,
            Reservation = new ReservationUpdateRequest
            {
                Status = reservation.Status,
                ExpiresAt = reservation.ExpiresAt
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminReservationEditViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.PutAsync<ReservationResponse>($"reservations/{id}", model.Reservation, cancellationToken);
            TempData["Success"] = "Reservation status was updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"reservations/{id}", cancellationToken);
            TempData["Success"] = "Reservation was deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Reservation could not be deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"reservations/{id}/force", cancellationToken);
            TempData["Success"] = "Reservation and related tickets/payments were force deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Reservation could not be force deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminReservationDetailsViewModel> BuildDetailsAsync(int id, CancellationToken cancellationToken)
    {
        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{id}", cancellationToken);
        var tickets = await _apiClient.GetAsync<PagedResult<TicketResponse>>($"tickets?reservationId={id}&pageSize=100", cancellationToken);
        var payments = await _apiClient.GetAsync<PagedResult<PaymentResponse>>($"payments?reservationId={id}&pageSize=100&sortBy=createdAt&sortDirection=desc", cancellationToken);
        var flight = await _apiClient.GetAsync<FlightResponse>($"flights/{reservation.FlightId}", cancellationToken);
        var user = await _apiClient.GetAsync<UserResponse>($"users/{reservation.UserId}", cancellationToken);
        var airports = await _apiClient.GetAsync<PagedResult<AirportResponse>>("airports?pageSize=100", cancellationToken);

        return new AdminReservationDetailsViewModel
        {
            Reservation = reservation,
            User = user,
            Flight = flight,
            DepartureAirport = airports.Items.FirstOrDefault(x => x.Id == flight.DepartureAirportId),
            ArrivalAirport = airports.Items.FirstOrDefault(x => x.Id == flight.ArrivalAirportId),
            Tickets = tickets.Items,
            Payments = payments.Items
        };
    }

    private async Task<IReadOnlyCollection<UserResponse>> LoadUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _apiClient.GetAsync<PagedResult<UserResponse>>("users?pageSize=100&sortBy=email", cancellationToken);
        return users.Items;
    }

    private async Task<IReadOnlyCollection<FlightResponse>> LoadFlightsAsync(CancellationToken cancellationToken)
    {
        var flights = await _apiClient.GetAsync<PagedResult<FlightResponse>>("flights?pageSize=100&sortBy=flightNumber", cancellationToken);
        return flights.Items;
    }
}
