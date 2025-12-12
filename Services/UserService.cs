using weatherapp.Data;
using weatherapp.DTO;
using Microsoft.EntityFrameworkCore;

namespace weatherapp.Services
{
    public class UserService(WeatherDbContext context) : IUserService
    {
        public async Task<PagedResponseDto<UserListDto>> GetUsersAsync(int pageNumber, int pageSize)
        {
            var totalCount = await context.Users.CountAsync();
            
            var users = await context.Users
                .OrderBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Role = u.Roles ?? "User"
                })
                .ToListAsync();

            return new PagedResponseDto<UserListDto>
            {
                Data = users,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}

