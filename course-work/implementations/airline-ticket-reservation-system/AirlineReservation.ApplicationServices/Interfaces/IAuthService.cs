using AirlineReservation.Contracts.Requests;
using AirlineReservation.Contracts.Responses;

namespace AirlineReservation.ApplicationServices.Interfaces;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
