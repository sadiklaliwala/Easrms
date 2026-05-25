using Easrms.Common.Constants;

namespace Easrms.Application.DTOs.Auth;

public class LinkedProviderDto
{
    public Guid Id { get; set; }
    public AuthProviderEnum AuthProvider { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime LinkedOn { get; set; }
}
