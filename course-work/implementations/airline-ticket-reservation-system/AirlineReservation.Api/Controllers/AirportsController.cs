using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[Route("api/airports")]
public class AirportsController : CrudControllerBase<AirportFilterRequest, AirportCreateRequest, AirportUpdateRequest, AirportResponse>
{
    public AirportsController(IAirportService service) : base(service)
    {
    }
}
