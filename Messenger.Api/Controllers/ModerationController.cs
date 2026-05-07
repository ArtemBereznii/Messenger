namespace Messenger.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messenger.Api.Data;
using Messenger.Api.Models;

[ApiController]
public class ModerationController : ControllerBase
{
    private readonly MessengerContext _context;

    public ModerationController(MessengerContext context)
    {
        _context = context;
    }

    [HttpPost("messages/{messageId}/report")]
    public async Task<IActionResult> ReportMessage(Guid messageId, [FromBody] SubmitReportRequest request)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            return NotFound("Message not found.");

        var userExists = await _context.Users.AnyAsync(u => u.Id == request.ReporterId);
        if (!userExists)
            return NotFound("Reporter user not found.");

        var report = new Report
        {
            MessageId = messageId,
            ReporterId = request.ReporterId,
            Reason = request.Reason
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return Accepted(new { message = "Report submitted.", reportId = report.Id });
    }

    [HttpPost("reports/{reportId}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request)
    {
        var report = await _context.Reports.FindAsync(reportId);
        if (report == null)
            return NotFound("Report not found.");

        if (report.IsResolved)
            return BadRequest("This report has already been resolved.");

        if (request.Action.ToUpper() == "HIDE")
        {
            var message = await _context.Messages.FindAsync(report.MessageId);
            if (message != null)
            {
                message.IsHidden = true;
                message.ModerationReason = report.Reason;
            }
        }

        report.IsResolved = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Report resolved with action: {request.Action.ToUpper()}" });
    }
}

public record SubmitReportRequest(Guid ReporterId, string Reason);
public record ResolveReportRequest(string Action); // "HIDE" or "DISMISS"