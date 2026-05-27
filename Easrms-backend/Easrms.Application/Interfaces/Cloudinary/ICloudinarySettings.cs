namespace Easrms.Application.Interfaces.Cloudinary;

public interface ICloudinarySettings
{
    string CloudName { get; init; }
    string ApiKey { get; init; }
    string ApiSecret { get; init; }
}
