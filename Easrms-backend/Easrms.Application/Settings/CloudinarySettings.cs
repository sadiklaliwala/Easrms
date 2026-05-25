namespace Easrms.Application.Settings;

public sealed class CloudinarySettings : Easrms.Application.Interfaces.ICloudinarySettings
{
    public string CloudName { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
}
