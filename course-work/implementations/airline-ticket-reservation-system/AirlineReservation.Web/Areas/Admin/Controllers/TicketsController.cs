using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class TicketsController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public TicketsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(AdminTicketIndexViewModel model, CancellationToken cancellationToken)
    {
        var path = QueryStringBuilder.Build(
            "tickets",
            ("reservationId", model.ReservationId),
            ("passengerLastName", model.PassengerLastName),
            ("ticketStatus", model.TicketStatus),
            ("pageSize", 100),
            ("sortBy", "passengerLastName"));

        model.Results = await _apiClient.GetAsync<PagedResult<TicketResponse>>(path, cancellationToken);
        model.Reservations = await LoadReservationsAsync(cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var ticket = await _apiClient.GetAsync<TicketResponse>($"tickets/{id}", cancellationToken);
        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{ticket.ReservationId}", cancellationToken);
        return View(new AdminTicketDetailsViewModel { Ticket = ticket, Reservation = reservation });
    }

    public async Task<IActionResult> Create(int? reservationId, CancellationToken cancellationToken)
    {
        var model = new AdminTicketFormViewModel();
        if (reservationId.HasValue)
        {
            model.Ticket.ReservationId = reservationId.Value;
        }

        return View(await BuildTicketFormAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminTicketFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildTicketFormAsync(model, cancellationToken));
        }

        try
        {
            var ticket = await _apiClient.PostAsync<TicketResponse>("tickets", model.Ticket, cancellationToken: cancellationToken);
            TempData["Success"] = "Ticket was created.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(await BuildTicketFormAsync(model, cancellationToken));
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var ticket = await _apiClient.GetAsync<TicketResponse>($"tickets/{id}", cancellationToken);
        return View(await BuildTicketFormAsync(new AdminTicketFormViewModel
        {
            Id = id,
            Ticket = new TicketCreateRequest
            {
                ReservationId = ticket.ReservationId,
                PassengerFirstName = ticket.PassengerFirstName,
                PassengerLastName = ticket.PassengerLastName,
                PassengerDocumentNumber = ticket.PassengerDocumentNumber,
                PassengerType = ticket.PassengerType,
                SeatNumber = ticket.SeatNumber,
                FareClass = ticket.FareClass,
                Price = ticket.Price,
                Currency = ticket.Currency,
                TicketStatus = ticket.TicketStatus,
                IssuedAt = ticket.IssuedAt
            }
        }, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminTicketFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(await BuildTicketFormAsync(model, cancellationToken));
        }

        try
        {
            await _apiClient.PutAsync<TicketResponse>($"tickets/{id}", model.Ticket, cancellationToken);
            TempData["Success"] = "Ticket was updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(await BuildTicketFormAsync(model, cancellationToken));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"tickets/{id}", cancellationToken);
            TempData["Success"] = "Ticket was deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Ticket could not be deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminTicketFormViewModel> BuildTicketFormAsync(AdminTicketFormViewModel model, CancellationToken cancellationToken)
    {
        model.Reservations = await LoadReservationsAsync(cancellationToken);
        return model;
    }

    private async Task<IReadOnlyCollection<ReservationResponse>> LoadReservationsAsync(CancellationToken cancellationToken)
    {
        var reservations = await _apiClient.GetAsync<PagedResult<ReservationResponse>>("reservations?pageSize=100&sortBy=bookingDate&sortDirection=desc", cancellationToken);
        return reservations.Items;
    }
}
