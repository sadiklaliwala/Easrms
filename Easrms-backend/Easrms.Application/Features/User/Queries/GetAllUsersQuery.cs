using Easrms.Application.DTOs.User;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.User.Queries;

public record class GetAllUsersQuery(UserQueryParams QueryParams) : IRequest<UserListWithPaginationDto>
{
    
     
}

public class GetAllUsersQueryHandler(IUserRepository userRepository) : IRequestHandler<GetAllUsersQuery, UserListWithPaginationDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    public async Task<UserListWithPaginationDto> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var queryParams = new UserQueryParams()
        {
            PageNumber =request.QueryParams.PageNumber,
            PageSize = request.QueryParams.PageSize,
            SortBy = request.QueryParams.SortBy,
            SortDirection = request.QueryParams.SortDirection,
            IsActive = request.QueryParams.IsActive,
            RoleId = request.QueryParams.RoleId,
            Search = request.QueryParams.Search,
            SortAscending = request.QueryParams.SortAscending
        };

        // Prefer explicit SortDirection string if provided, otherwise derive from SortAscending flag
        var sortDirection = !string.IsNullOrWhiteSpace(queryParams.SortDirection)
            ? queryParams.SortDirection
            : (queryParams.SortAscending ? "asc" : "desc");

        return await _userRepository.GetAllAsync(
            queryParams.PageNumber, 
            queryParams.PageSize,
            queryParams.Search, 
            queryParams.RoleId, 
            queryParams.IsActive, 
            queryParams.SortBy, 
            sortDirection, 
            cancellationToken);
    }
}

