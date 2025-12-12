using weatherapp.DTO;
using weatherapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace weatherapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService userService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<PagedResponseDto<UserListDto>>> GetUsers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 5)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 5;

            var result = await userService.GetUsersAsync(pageNumber, pageSize);
            return Ok(result);
        }
    }
}

