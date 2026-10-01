using Application.DTOs;

namespace Application.Services
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    }
}
