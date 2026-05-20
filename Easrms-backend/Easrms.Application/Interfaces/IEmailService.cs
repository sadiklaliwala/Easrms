namespace Easrms.Application.Interfaces;

public interface IEmailService
{
    Task SendRequestOpenedAsync(string toEmail, string requestNumber, string requestTitle);
    Task SendRequestResolvedAsync(string toEmail, string requestNumber, string requestTitle);
    Task SendSLANearingBreachAsync(string toEmail, string requestNumber, string requestTitle);
    Task SendSLABreachedAsync(string toEmail, string requestNumber, string requestTitle);
    Task SendRequestEscalatedAsync(string toEmail, string requestNumber, string requestTitle, string escalationReason);
}
