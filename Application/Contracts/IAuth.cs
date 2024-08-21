using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Contracts
{
    public interface IAuth
    {
        Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest registerUserRequest);
        Task<LoginResponse> LoginUserAsync(LoginRequest loginRequest);
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    }
}
