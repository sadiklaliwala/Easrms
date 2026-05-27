using Easrms.Application.DTOs.Cloudinary;
using Easrms.Application.Interfaces.Cloudinary;
using Easrms.Common.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CloudinaryController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

    public CloudinaryController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("sign")]
    [Authorize]
    public async Task<IActionResult> Sign(
        [FromBody] UploadSignatureDto? request)
    {
        var folder = request?.Folder;

        var (apiKey, cloudName, timestamp, signature) =
            await _cloudinaryService.CreateUploadSignatureAsync(folder);

        var dto = new UploadSignatureDto
        {
            ApiKey = apiKey,
            CloudName = cloudName,
            Timestamp = timestamp,
            Signature = signature,
            Folder = folder
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Cloudinary upload signature generated successfully.",
            Data = dto,
            Errors = null
        });
    }
}