namespace Easrms.Application.Interfaces.Email
{
    public interface IEmailTemplates
    {
        static abstract string PasswordResetOtpBody(string userName, string otp);
        static abstract string PasswordResetOtpSubject();
        static abstract string RequestEscalatedBody(string requestNumber, string requestTitle, string escalationReason);
        static abstract string RequestEscalatedSubject();
        static abstract string RequestOpenedBody(string requestNumber, string requestTitle);
        static abstract string RequestOpenedSubject();
        static abstract string RequestResolvedBody(string requestNumber, string requestTitle);
        static abstract string RequestResolvedSubject();
        static abstract string SLABreachedBody(string requestNumber, string requestTitle);
        static abstract string SLABreachedSubject();
        static abstract string SLANearingBreachBody(string requestNumber, string requestTitle);
        static abstract string SLANearingBreachSubject();
    }
}