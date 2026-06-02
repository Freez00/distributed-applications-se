using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Areas.Admin.Controllers;

public class PaymentsController : AdminControllerBase
{
    private readonly AirlineApiClient _apiClient;

    public PaymentsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(AdminPaymentIndexViewModel model, CancellationToken cancellationToken)
    {
        var path = QueryStringBuilder.Build(
            "payments",
            ("reservationId", model.ReservationId),
            ("paymentStatus", model.PaymentStatus),
            ("paymentMethod", model.PaymentMethod),
            ("transactionReference", model.TransactionReference),
            ("pageSize", 100),
            ("sortBy", "createdAt"),
            ("sortDirection", "desc"));

        model.Results = await _apiClient.GetAsync<PagedResult<PaymentResponse>>(path, cancellationToken);
        model.Reservations = await LoadReservationsAsync(cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var payment = await _apiClient.GetAsync<PaymentResponse>($"payments/{id}", cancellationToken);
        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{payment.ReservationId}", cancellationToken);
        return View(new AdminPaymentDetailsViewModel { Payment = payment, Reservation = reservation });
    }

    public async Task<IActionResult> Create(int? reservationId, CancellationToken cancellationToken)
    {
        var model = new AdminPaymentFormViewModel();
        if (reservationId.HasValue)
        {
            model.Payment.ReservationId = reservationId.Value;
            await ApplyReservationDefaultsAsync(model, reservationId.Value, cancellationToken);
        }

        return View(await BuildPaymentFormAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminPaymentFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildPaymentFormAsync(model, cancellationToken));
        }

        try
        {
            var payment = await _apiClient.PostAsync<PaymentResponse>("payments", model.Payment, cancellationToken: cancellationToken);
            TempData["Success"] = "Payment was created.";
            return RedirectToAction(nameof(Details), new { id = payment.Id });
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(await BuildPaymentFormAsync(model, cancellationToken));
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var payment = await _apiClient.GetAsync<PaymentResponse>($"payments/{id}", cancellationToken);
        return View(await BuildPaymentFormAsync(new AdminPaymentFormViewModel
        {
            Id = id,
            Payment = new PaymentCreateRequest
            {
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionReference = payment.TransactionReference,
                PaidAt = payment.PaidAt
            }
        }, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminPaymentFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(await BuildPaymentFormAsync(model, cancellationToken));
        }

        try
        {
            await _apiClient.PutAsync<PaymentResponse>($"payments/{id}", model.Payment, cancellationToken);
            TempData["Success"] = "Payment was updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(await BuildPaymentFormAsync(model, cancellationToken));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync($"payments/{id}", cancellationToken);
            TempData["Success"] = "Payment was deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Payment could not be deleted. {ex.FriendlyMessage}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminPaymentFormViewModel> BuildPaymentFormAsync(AdminPaymentFormViewModel model, CancellationToken cancellationToken)
    {
        model.Reservations = await LoadReservationsAsync(cancellationToken);
        return model;
    }

    private async Task ApplyReservationDefaultsAsync(AdminPaymentFormViewModel model, int reservationId, CancellationToken cancellationToken)
    {
        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{reservationId}", cancellationToken);
        model.Payment.Amount = reservation.TotalPrice;
        model.Payment.Currency = reservation.Currency;
    }

    private async Task<IReadOnlyCollection<ReservationResponse>> LoadReservationsAsync(CancellationToken cancellationToken)
    {
        var reservations = await _apiClient.GetAsync<PagedResult<ReservationResponse>>("reservations?pageSize=100&sortBy=bookingDate&sortDirection=desc", cancellationToken);
        return reservations.Items;
    }
}
