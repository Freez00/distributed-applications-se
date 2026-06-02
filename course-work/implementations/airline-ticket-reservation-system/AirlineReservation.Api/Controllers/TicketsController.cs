using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[Route("api/tickets")]
public class TicketsController : CrudControllerBase<TicketFilterRequest, TicketCreateRequest, TicketUpdateRequest, TicketResponse>
{
    public TicketsController(ITicketService service) : base(service)
    {
    }
}
