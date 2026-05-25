namespace Easrms.Application.DTOs.Cloudinary;

public class UploadSignatureDto
{
    public string ApiKey { get; init; } = string.Empty;
    public string CloudName { get; init; } = string.Empty;
    public long Timestamp { get; init; }
    public string Signature { get; init; } = string.Empty;
    public string? Folder { get; init; }
}
