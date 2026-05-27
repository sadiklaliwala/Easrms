using Easrms.Application.DTOs.Category;
using Easrms.Application.Features.Category.Commands;
using Easrms.Application.Features.Category.Queries;
using Easrms.Common.Constants;
using Easrms.Common.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/categories
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isApprovalRequired = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        [FromQuery] bool? sortAscending = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllCategoriesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            IsActive = isActive,
            IsApprovalRequired = isApprovalRequired,
            SortBy = sortBy,
            SortDirection = sortDirection,
            SortAscending = sortAscending
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Categories retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // GET /api/categories/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryByIdQuery { CategoryId = id };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Category retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // POST /api/categories
    [HttpPost]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCategoryCommand
        {
            CategoryName = dto.CategoryName,
            IsApprovalRequired = dto.IsApprovalRequired,
            SLAHours = dto.SLAHours,
        };

        var newId = await _mediator.Send(command, cancellationToken);

        return StatusCode(201, new ApiResponse<object>
        {
            Success = true,
            StatusCode = 201,
            Message = "Category created successfully.",
            Data = new { CategoryId = newId },
            Errors = null
        });
    }

    // PUT /api/categories/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = id,
            CategoryName = dto.CategoryName,
            IsApprovalRequired = dto.IsApprovalRequired,
            SLAHours= dto.SLAHours
        };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Category updated successfully.",
            Data = null,
            Errors = null
        });
    }

    // PUT /api/categories/{id}/activate-deactivate
    [HttpPut("{id:guid}/activate-deactivate")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> ToggleStatus(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ToggleCategoryStatusCommand { CategoryId = id };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Category status updated successfully.",
            Data = null,
            Errors = null
        });
    }

    // POST /api/categories/bulk
    [HttpPost("bulk")]
    [Authorize(Roles = RoleConstants.Admin)]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> BulkUpload(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null) return BadRequest(ApiResponse<object>.FailResponse("No file provided", 400));

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);

        var command = new Easrms.Application.Features.Category.Commands.BulkCreateCategories.BulkCreateCategoriesCommand
        {
            FileName = file.FileName,
            FileContent = ms.ToArray()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // DELETE /api/categories/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new Easrms.Application.Features.Category.Commands.DeleteCategory.DeleteCategoryCommand { CategoryId = id };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}