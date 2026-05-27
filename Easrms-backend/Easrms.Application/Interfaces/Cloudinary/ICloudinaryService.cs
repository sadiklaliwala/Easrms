namespace Easrms.Application.Interfaces.Cloudinary;

public interface ICloudinaryService
{
    /// <summary>
    /// Generate a Cloudinary upload signature for client-side signed uploads.
    /// </summary>
    /// <param name="folder">Optional folder name to include in the signature.</param>
    /// <returns>A tuple containing apiKey, cloudName, timestamp and signature.</returns>
    Task<(string ApiKey, string CloudName, long Timestamp, string Signature)> CreateUploadSignatureAsync(string? folder = null);
}
