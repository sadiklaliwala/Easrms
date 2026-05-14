using Easrms.Application.DTOs.Common;

namespace Easrms.Application.DTOs.User;

public class UserListWithPaginationDto
{
    public List<UserListDto> Items { get; set; } = new List<UserListDto>();
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}
