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
                return BadRequest(new { message = "User with this email already exists" });

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
                return BadRequest(new { message = "Invalid email or password." });

            // Set access token as httpOnly cookie
            Response.Cookies.Append("accessToken", result.AccessToken, authService.GetAccessTokenCookieOptions());

            // Set refresh token as httpOnly cookie
            Response.Cookies.Append("refreshToken", result.RefreshToken, authService.GetRefreshTokenCookieOptions());

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
                return Unauthorized(new { message = "Refresh token not found." });

            var request = new RefreshTokenRequestDto 
            { 
                UserId = 0, //find user by refresh token
                RefreshToken = refreshToken 
            };
            
            var result = await authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized(new { message = "Invalid refresh token." });

            // Update cookies with new tokens
            Response.Cookies.Append("accessToken", result.AccessToken, authService.GetAccessTokenCookieOptions());
            Response.Cookies.Append("refreshToken", result.RefreshToken, authService.GetRefreshTokenCookieOptions());

            return Ok(new { message = "Token refreshed successfully" });
        }

 
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                await authService.LogoutAsync(userId);
            }

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
