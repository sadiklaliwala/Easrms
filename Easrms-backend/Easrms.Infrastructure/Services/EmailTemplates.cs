namespace Easrms.Infrastructure.Services;

internal static class EmailTemplates
{
    // ── Request Opened ────────────────────────────────────────────────────────

    public static string RequestOpenedSubject()
        => "Support Case Opened";

    public static string RequestOpenedBody(string requestNumber, string requestTitle)
        => $"""
           <!DOCTYPE html>
           <html>
           <body style="font-family:Arial,sans-serif;color:#333;font-size:15px;line-height:1.6;max-width:600px;margin:0 auto;padding:24px;">
               <p>Thank you for contacting Support Team - a support case has been opened for you.</p>
               <ul style="list-style:disc;padding-left:20px;margin:12px 0;">
                   <li><strong>Case ID #:</strong> {requestNumber}</li>
                   <li><strong>Title:</strong> {requestTitle}</li>
               </ul>
               <p>Thanks,<br/>Support Team</p>
           </body>
           </html>
           """;

    // ── Request Resolved ──────────────────────────────────────────────────────

    public static string RequestResolvedSubject()
        => "Your Support Case Has Been Resolved - Action Required";

    public static string RequestResolvedBody(string requestNumber, string requestTitle)
        => $"""
           <!DOCTYPE html>
           <html>
           <body style="font-family:Arial,sans-serif;color:#333;font-size:15px;line-height:1.6;max-width:600px;margin:0 auto;padding:24px;">
               <p>Your support case has been resolved. Please log in to review the resolution and close the case.</p>
               <ul style="list-style:disc;padding-left:20px;margin:12px 0;">
                   <li><strong>Case ID #:</strong> {requestNumber}</li>
                   <li><strong>Title:</strong> {requestTitle}</li>
               </ul>
               <p>Thanks,<br/>Support Team</p>
           </body>
           </html>
           """;

    // ── SLA Nearing Breach ─────────────────────────────────────────────────────

    public static string SLANearingBreachSubject()
        => "SLA Nearing Breach - Action Required";

    public static string SLANearingBreachBody(string requestNumber, string requestTitle)
        => $"""
           <!DOCTYPE html>
           <html>
           <body style="font-family:Arial,sans-serif;color:#333;font-size:15px;line-height:1.6;max-width:600px;margin:0 auto;padding:24px;">
               <p>This support case is nearing its SLA breach time and requires immediate attention.</p>
               <ul style="list-style:disc;padding-left:20px;margin:12px 0;">
                   <li><strong>Case ID #:</strong> {requestNumber}</li>
                   <li><strong>Title:</strong> {requestTitle}</li>
               </ul>
               <p>Please take immediate action to avoid SLA breach.<br/>Thanks,<br/>Support Team</p>
           </body>
           </html>
           """;


    // ── SLA Breached ──────────────────────────────────────────────────────────

    public static string SLABreachedSubject()
        => "SLA Breached";

    public static string SLABreachedBody(string requestNumber, string requestTitle)
        => $"""
           <!DOCTYPE html>
           <html>
           <body style="font-family:Arial,sans-serif;color:#333;font-size:15px;line-height:1.6;max-width:600px;margin:0 auto;padding:24px;">
               <p>This support case has breached its SLA. Please review and act immediately.</p>
               <ul style="list-style:disc;padding-left:20px;margin:12px 0;">
                   <li><strong>Case ID #:</strong> {requestNumber}</li>
                   <li><strong>Title:</strong> {requestTitle}</li>
               </ul>
               <p>Thanks,<br/>Support Team</p>
           </body>
           </html>
           """;

    // ── Request Escalated ─────────────────────────────────────────────────────

    public static string RequestEscalatedSubject()
        => "Support Case Escalated";

    public static string RequestEscalatedBody(string requestNumber, string requestTitle, string escalationReason)
        => $"""
           <!DOCTYPE html>
           <html>
           <body style="font-family:Arial,sans-serif;color:#333;font-size:15px;line-height:1.6;max-width:600px;margin:0 auto;padding:24px;">
               <p>This support case has been escalated. See the reason below and take the necessary action.</p>
               <ul style="list-style:disc;padding-left:20px;margin:12px 0;">
                   <li><strong>Case ID #:</strong> {requestNumber}</li>
                   <li><strong>Title:</strong> {requestTitle}</li>
                   <li><strong>Reason:</strong> {escalationReason}</li>
               </ul>
               <p>Thanks,<br/>Support Team</p>
           </body>
           </html>
           """;
}
