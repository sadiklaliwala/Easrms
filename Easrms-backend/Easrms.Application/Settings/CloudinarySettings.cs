using Easrms.Application.Interfaces.Cloudinary;

namespace Easrms.Application.Settings;

public sealed class CloudinarySettings : ICloudinarySettings
{
    public string CloudName { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
}
