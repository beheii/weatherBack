namespace weatherapp.DTO;

public class PagedResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class UserListDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}

