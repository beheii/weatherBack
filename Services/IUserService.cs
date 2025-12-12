using weatherapp.DTO;

namespace weatherapp.Services
{
    public interface IUserService
    {
        Task<PagedResponseDto<UserListDto>> GetUsersAsync(int pageNumber, int pageSize);
    }
}

