using Easrms.Application.DTOs.Request;

namespace Easrms.Application.Interfaces;

public interface IExportService
{
    byte[] ExportRequestListToExcel(IReadOnlyList<RequestListDto> requests);
    byte[] ExportRequestListToPdf(IReadOnlyList<RequestListDto> requests);
    byte[] ExportRequestDetailToExcel(RequestDetailDto request);
    byte[] ExportRequestDetailToPdf(RequestDetailDto request);
}