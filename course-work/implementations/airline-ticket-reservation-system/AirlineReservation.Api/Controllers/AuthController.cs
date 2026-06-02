using System.Security.Claims;
using AirlineReservation.Api.Infrastructure;
using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(IAuthService authService, IUserService userService, JwtTokenService jwtTokenService)
    {
        _authService = authService;
        _userService = userService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterAsync(request, cancellationToken);
        var token = _jwtTokenService.CreateToken(user);
        return CreatedAtAction(nameof(Me), new { }, new AuthResponse
        {
            User = user,
            Token = token.Token,
            ExpiresAt = token.ExpiresAt
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.ValidateCredentialsAsync(request, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var token = _jwtTokenService.CreateToken(user);
        return Ok(new AuthResponse
        {
            User = user,
            Token = token.Token,
            ExpiresAt = token.ExpiresAt
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetByIdAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
