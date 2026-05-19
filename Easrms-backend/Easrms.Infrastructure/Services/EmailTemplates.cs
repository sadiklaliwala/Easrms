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
}
