using System.Collections.Generic;

namespace Easrms.Application.DTOs.Common;

public class BulkUploadResultDto
{
    public int TotalRows { get; set; }
    public int InsertedCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkUploadErrorDto> Errors { get; set; } = new();
}

public class BulkUploadErrorDto
{
    public int Row { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
