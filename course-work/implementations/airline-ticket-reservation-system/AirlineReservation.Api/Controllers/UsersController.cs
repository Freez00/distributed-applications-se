using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[Route("api/users")]
public class UsersController : CrudControllerBase<UserFilterRequest, UserCreateRequest, UserUpdateRequest, UserResponse>
{
    public UsersController(IUserService service) : base(service)
    {
    }
}
