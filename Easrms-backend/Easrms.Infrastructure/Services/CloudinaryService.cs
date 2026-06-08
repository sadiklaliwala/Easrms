using Easrms.Application.Interfaces.Cloudinary;
using System.Security.Cryptography;
using System.Text;

namespace Easrms.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly ICloudinarySettings _settings;

    public CloudinaryService(ICloudinarySettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public Task<(string ApiKey, string CloudName, long Timestamp, string Signature)> CreateUploadSignatureAsync(string? folder = null)
    {
        // Cloudinary signatures use a SHA-1 of the sorted params (non-empty) concatenated with the api secret
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var parameters = new List<string>
        {
            $"timestamp={timestamp}"
        };

        if (!string.IsNullOrWhiteSpace(folder))
        {
            parameters.Add($"folder={folder}");
        }

        // Sort parameters lexically by key (which is already the format "key=value")
        parameters.Sort(StringComparer.Ordinal);

        var payload = string.Join("&", parameters);

        var signature = ComputeSha1Hash(payload + _settings.ApiSecret);

        return Task.FromResult((_settings.ApiKey, _settings.CloudName, timestamp, signature));
    }

    private static string ComputeSha1Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA1.HashData(bytes);
        return string.Concat(hash.Select(b => b.ToString("x2")));
    }
}
