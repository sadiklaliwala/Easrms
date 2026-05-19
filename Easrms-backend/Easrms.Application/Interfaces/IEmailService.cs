namespace Easrms.Infrastructure.Services;

public interface IEmailService
{
    Task SendRequestOpenedAsync(string toEmail, string requestNumber, string requestTitle);
    Task SendRequestResolvedAsync(string toEmail, string requestNumber, string requestTitle);
}
