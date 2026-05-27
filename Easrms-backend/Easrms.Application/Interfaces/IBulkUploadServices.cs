using Easrms.Application.DTOs.Common;

namespace Easrms.Application.Interfaces;

public interface IUserBulkUploadService
{
    Task<BulkUploadResultDto> ProcessAsync(System.IO.Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

public interface ICategoryBulkUploadService
{
    Task<BulkUploadResultDto> ProcessAsync(System.IO.Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}

public interface IRequestBulkUploadService
{
    Task<BulkUploadResultDto> ProcessAsync(System.IO.Stream fileStream, string fileName, Guid employeeId, CancellationToken cancellationToken = default);
}
