using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[Route("api/aircraft")]
public class AircraftController : CrudControllerBase<AircraftFilterRequest, AircraftCreateRequest, AircraftUpdateRequest, AircraftResponse>
{
    public AircraftController(IAircraftService service) : base(service)
    {
    }
}
