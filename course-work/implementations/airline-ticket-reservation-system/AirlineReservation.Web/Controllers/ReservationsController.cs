using AirlineReservation.Contracts.Common;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using AirlineReservation.Web.Infrastructure;
using AirlineReservation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Web.Controllers;

public class ReservationsController : Controller
{
    private readonly AirlineApiClient _apiClient;

    public ReservationsController(AirlineApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = this.CurrentUserId();
        if (userId is null)
        {
            return this.RedirectToLogin();
        }

        var path = QueryStringBuilder.Build(
            "reservations",
            ("userId", userId),
            ("pageNumber", 1),
            ("pageSize", 50),
            ("sortBy", "bookingDate"),
            ("sortDirection", "desc"));

        var reservations = await _apiClient.GetAsync<PagedResult<ReservationResponse>>(path, cancellationToken);
        var flights = await _apiClient.GetAsync<PagedResult<FlightResponse>>("flights?pageSize=100&sortBy=flightNumber", cancellationToken);

        return View(new ReservationListViewModel
        {
            Reservations = reservations.Items,
            Flights = flights.Items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = this.CurrentUserId();
        if (userId is null)
        {
            return this.RedirectToLogin();
        }

        var reservation = await _apiClient.GetAsync<ReservationResponse>($"reservations/{id}", cancellationToken);
        if (reservation.UserId != userId.Value && HttpContext.Session.GetString(SessionKeys.UserRole) != "Admin")
        {
            return Forbid();
        }

        var tickets = await _apiClient.GetAsync<PagedResult<TicketResponse>>(
            $"tickets?reservationId={reservation.Id}&pageSize=20&sortBy=passengerLastName",
            cancellationToken);

        var payments = await _apiClient.GetAsync<PagedResult<PaymentResponse>>(
            $"payments?reservationId={reservation.Id}&pageSize=20&sortBy=createdAt&sortDirection=desc",
            cancellationToken);

        var flight = await _apiClient.GetAsync<FlightResponse>($"flights/{reservation.FlightId}", cancellationToken);
        var airports = await _apiClient.GetAsync<PagedResult<AirportResponse>>("airports?pageSize=100", cancellationToken);

        return View(new ReservationDetailsViewModel
        {
            Reservation = reservation,
            Flight = flight,
            DepartureAirport = airports.Items.FirstOrDefault(x => x.Id == flight.DepartureAirportId),
            ArrivalAirport = airports.Items.FirstOrDefault(x => x.Id == flight.ArrivalAirportId),
            Tickets = tickets.Items,
            Payments = payments.Items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int flightId, CancellationToken cancellationToken)
    {
        if (!this.IsSignedIn())
        {
            return this.RedirectToLogin();
        }

        var model = new CreateReservationViewModel
        {
            FlightId = flightId,
            Flight = await _apiClient.GetAsync<FlightResponse>($"flights/{flightId}", cancellationToken)
        };
        model.EnsureTicketSlots();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReservationViewModel model, CancellationToken cancellationToken)
    {
        var userId = this.CurrentUserId();
        if (userId is null)
        {
            return this.RedirectToLogin();
        }

        model.EnsureTicketSlots();
        await LoadFlightAsync(model, cancellationToken);
        ValidateTicketDrafts(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var activeTickets = model.Tickets
                .Take(model.PassengerCount)
                .Select(x => new ReservationTicketDraftRequest
                {
                    PassengerFirstName = x.PassengerFirstName!.Trim(),
                    PassengerLastName = x.PassengerLastName!.Trim(),
                    PassengerDocumentNumber = x.PassengerDocumentNumber!.Trim(),
                    PassengerType = x.PassengerType,
                    SeatNumber = string.IsNullOrWhiteSpace(x.SeatNumber) ? null : x.SeatNumber.Trim(),
                    FareClass = x.FareClass
                })
                .ToList();

            var reservation = await _apiClient.PostAsync<ReservationResponse>(
                "reservations",
                new ReservationCreateRequest
                {
                    UserId = userId.Value,
                    FlightId = model.FlightId,
                    Tickets = activeTickets
                },
                authorize: true,
                cancellationToken);

            TempData["Success"] = $"Reservation {reservation.ReservationCode} was created.";
            return RedirectToAction(nameof(Details), new { id = reservation.Id });
        }
        catch (ApiException ex)
        {
            foreach (var message in ex.FriendlyMessages)
            {
                ModelState.AddModelError(string.Empty, $"Reservation failed. {message}");
            }

            return View(model);
        }
    }

    private async Task LoadFlightAsync(CreateReservationViewModel model, CancellationToken cancellationToken)
    {
        model.Flight = await _apiClient.GetAsync<FlightResponse>($"flights/{model.FlightId}", cancellationToken);
    }

    private void ValidateTicketDrafts(CreateReservationViewModel model)
    {
        for (var i = 0; i < model.PassengerCount; i++)
        {
            var ticket = model.Tickets[i];
            if (string.IsNullOrWhiteSpace(ticket.PassengerFirstName))
            {
                ModelState.AddModelError($"Tickets[{i}].PassengerFirstName", "First name is required.");
            }
            else if (ticket.PassengerFirstName.Trim().Length < 2)
            {
                ModelState.AddModelError($"Tickets[{i}].PassengerFirstName", "First name must be at least 2 characters.");
            }

            if (string.IsNullOrWhiteSpace(ticket.PassengerLastName))
            {
                ModelState.AddModelError($"Tickets[{i}].PassengerLastName", "Last name is required.");
            }
            else if (ticket.PassengerLastName.Trim().Length < 2)
            {
                ModelState.AddModelError($"Tickets[{i}].PassengerLastName", "Last name must be at least 2 characters.");
            }

            if (string.IsNullOrWhiteSpace(ticket.PassengerDocumentNumber))
            {
                ModelState.AddModelError($"Tickets[{i}].PassengerDocumentNumber", "Document number is required.");
            }
            else if (ticket.PassengerDocumentNumber.Trim().Length < 5)
            {
                ModelState.AddModelError($"Tickets[{i}].PassengerDocumentNumber", "Document number must be at least 5 characters.");
            }
        }
    }
}
