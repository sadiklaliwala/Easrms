using Easrms.Application.DTOs.Common;

namespace Easrms.Application.DTOs.Request;

public class RequestListWithPaginationDto
{
    public List<RequestListDto> Items { get; set; } = new List<RequestListDto>();
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}

