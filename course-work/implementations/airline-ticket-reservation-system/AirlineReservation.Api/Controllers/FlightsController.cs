using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[Route("api/flights")]
public class FlightsController : CrudControllerBase<FlightFilterRequest, FlightCreateRequest, FlightUpdateRequest, FlightResponse>
{
    public FlightsController(IFlightService service) : base(service)
    {
    }
}
