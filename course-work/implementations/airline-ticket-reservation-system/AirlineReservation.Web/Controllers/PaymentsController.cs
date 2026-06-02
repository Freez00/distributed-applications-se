using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Controllers;

public class PaymentsController : Controller
{
    private readonly AirlineApiClient _apiClient;

    public PaymentsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int reservationId, CancellationToken cancellationToken)
    {
        if (!this.IsSignedIn())
        {
            return this.RedirectToLogin();
        }

        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{reservationId}", cancellationToken);
        if (!CanRecordPayment(reservation))
        {
            TempData["Success"] = "Payments can only be recorded for pending reservations.";
            return RedirectToAction("Details", "Reservations", new { id = reservationId });
        }

        if (await HasCompletedPaymentAsync(reservationId, cancellationToken))
        {
            TempData["Success"] = "This reservation already has a completed payment.";
            return RedirectToAction("Details", "Reservations", new { id = reservationId });
        }

        return View(new CreatePaymentViewModel
        {
            ReservationId = reservation.Id,
            Reservation = reservation,
            Amount = reservation.TotalPrice,
            Currency = reservation.Currency,
            TransactionReference = $"PAY-{reservation.ReservationCode}"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaymentViewModel model, CancellationToken cancellationToken)
    {
        if (!this.IsSignedIn())
        {
            return this.RedirectToLogin();
        }

        await LoadReservationAsync(model, cancellationToken);
        if (!CanRecordPayment(model.Reservation!))
        {
            TempData["Success"] = "Payments can only be recorded for pending reservations.";
            return RedirectToAction("Details", "Reservations", new { id = model.ReservationId });
        }

        if (await HasCompletedPaymentAsync(model.ReservationId, cancellationToken))
        {
            TempData["Success"] = "This reservation already has a completed payment.";
            return RedirectToAction("Details", "Reservations", new { id = model.ReservationId });
        }

        model.Amount = model.Reservation!.TotalPrice;
        model.Currency = model.Reservation.Currency;
        ModelState.Remove(nameof(CreatePaymentViewModel.Amount));
        ModelState.Remove(nameof(CreatePaymentViewModel.Currency));

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _apiClient.PostAsync<PaymentResponse>(
                "payments",
                new PaymentCreateRequest
                {
                    ReservationId = model.ReservationId,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "Completed",
                    TransactionReference = model.TransactionReference,
                    PaidAt = DateTime.UtcNow
                },
                authorize: true,
                cancellationToken);

            TempData["Success"] = "Payment was recorded.";
            return RedirectToAction("Details", "Reservations", new { id = model.ReservationId });
        }
        catch (ApiException ex)
        {
            await LoadReservationAsync(model, cancellationToken);
            foreach (var message in ex.FriendlyMessages)
            {
                ModelState.AddModelError(string.Empty, $"Payment failed. {message}");
            }

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(int paymentId, int reservationId, CancellationToken cancellationToken)
    {
        var userId = this.CurrentUserId();
        if (userId is null)
        {
            return this.RedirectToLogin();
        }

        var payment = await _apiClient.GetAsync<PaymentResponse>($"payments/{paymentId}", cancellationToken);
        if (payment.ReservationId != reservationId)
        {
            return BadRequest();
        }

        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{reservationId}", cancellationToken);
        if (reservation.UserId != userId.Value && HttpContext.Session.GetString(SessionKeys.UserRole) != "Admin")
        {
            return Forbid();
        }

        if (!payment.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Success"] = "Only completed payments can be refunded.";
            return RedirectToAction("Details", "Reservations", new { id = reservationId });
        }

        try
        {
            await _apiClient.PutAsync<PaymentResponse>(
                $"payments/{paymentId}",
                new PaymentUpdateRequest
                {
                    ReservationId = payment.ReservationId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentStatus = "Refunded",
                    TransactionReference = payment.TransactionReference,
                    PaidAt = payment.PaidAt
                },
                cancellationToken);

            TempData["Success"] = "Payment was refunded and the reservation was cancelled.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"Refund failed. {ex.FriendlyMessage}";
        }

        return RedirectToAction("Details", "Reservations", new { id = reservationId });
    }

    private async Task LoadReservationAsync(CreatePaymentViewModel model, CancellationToken cancellationToken)
    {
        model.Reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{model.ReservationId}", cancellationToken);
    }

    private async Task<bool> HasCompletedPaymentAsync(int reservationId, CancellationToken cancellationToken)
    {
        var payments = await _apiClient.GetAsync<AirlineReservation.Contracts.Common.PagedResult<PaymentResponse>>(
            $"payments?reservationId={reservationId}&paymentStatus=Completed&pageSize=1",
            cancellationToken);

        return payments.TotalItems > 0;
    }

    private static bool CanRecordPayment(ReservationResponse reservation)
        => reservation.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
}
