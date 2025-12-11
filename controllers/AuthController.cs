using weatherapp.Models;
using weatherapp.Services;
using weatherapp.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace weatherapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
                return BadRequest("Email already exists.");

            // Don't return the password hash
            return Ok(new 
            { 
                userId = user.UserId, 
                email = user.Email, 
                role = user.Roles,
                message = "User registered successfully" 
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserDto request)
        {
            var result = await authService.LoginAsync(request);
            if (result is null)
                return BadRequest("Invalid email or password.");

            // Set access token as httpOnly cookie
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,           // Prevents XSS attacks - JS can't access
                Secure = !isDevelopment,   // Only sent over HTTPS (except in development)
                SameSite = SameSiteMode.Strict, // CSRF protection
                Expires = DateTime.UtcNow.AddMinutes(15), // Match token expiry
                Path = "/"
            };
            Response.Cookies.Append("accessToken", result.AccessToken, accessTokenCookieOptions);

            // Set refresh token as httpOnly cookie
            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7), // Match refresh token expiry
                Path = "/"
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken, refreshTokenCookieOptions);

            // Return user info (not tokens)
            return Ok(new 
            { 
                message = "Login successful",
                email = request.Email
            });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            // Get refresh token from cookie
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token not found.");

            // AuthService will find user by refresh token (userId = 0 means find by token)
            var request = new RefreshTokenRequestDto 
            { 
                UserId = 0, // 0 means find user by refresh token
                RefreshToken = refreshToken 
            };
            
            var result = await authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");

            // Update cookies with new tokens
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Path = "/"
            };
            Response.Cookies.Append("accessToken", result.AccessToken, accessTokenCookieOptions);

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken, refreshTokenCookieOptions);

            return Ok(new { message = "Token refreshed successfully" });
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are an admin!");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Try to get userId from claims if authenticated, otherwise proceed with cookie cleanup
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                await authService.LogoutAsync(userId);
            }

            // Clear cookies regardless of authentication status
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new
            {
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                Claims = claims
            });
        }
    }
}
