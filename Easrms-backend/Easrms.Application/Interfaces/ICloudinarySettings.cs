namespace Easrms.Application.Interfaces;

public interface ICloudinarySettings
{
    string CloudName { get; init; }
    string ApiKey { get; init; }
    string ApiSecret { get; init; }
}
