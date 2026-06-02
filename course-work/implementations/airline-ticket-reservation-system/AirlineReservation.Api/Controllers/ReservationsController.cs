using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[Route("api/reservations")]
public class ReservationsController : CrudControllerBase<ReservationFilterRequest, ReservationCreateRequest, ReservationUpdateRequest, ReservationResponse>
{
    public ReservationsController(IReservationService service) : base(service)
    {
    }
}
