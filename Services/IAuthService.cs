using weatherapp.DTO;
using weatherapp.Models;
using Microsoft.AspNetCore.Http;

namespace weatherapp.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(int userId);
        CookieOptions GetAccessTokenCookieOptions();
        CookieOptions GetRefreshTokenCookieOptions();
    }
}
