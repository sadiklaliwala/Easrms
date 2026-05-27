using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<ApiResponse<string>>
{
    public Guid CategoryId { get; set; }
}
