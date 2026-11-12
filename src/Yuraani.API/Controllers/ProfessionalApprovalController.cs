using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuraani.Application.Features.ProfessionalApproval.Commands.ReviewProfessionalApproval;
using Yuraani.Application.Features.ProfessionalApproval.Commands.SubmitProfessionalApproval;
using Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;
using Yuraani.Domain.ProfessionalApproval;
namespace Yuraani.API.Controllers;
[ApiController]
[Route("api/professional-approvals")]
[Authorize]
public sealed class ProfessionalApprovalController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitApprovalRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var id = await sender.Send(new SubmitProfessionalApprovalCommand(userId, request.ProfessionalType), ct);
        return Accepted(new { approvalId = id });
    }
    [HttpGet("pending")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Pending(CancellationToken ct) =>
        Ok(await sender.Send(new GetPendingApprovalsQuery(), ct));
    [HttpPost("{approvalId:guid}/decision")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Decide(Guid approvalId, [FromBody] ReviewApprovalRequest request, CancellationToken ct)
    {
        await sender.Send(new ReviewProfessionalApprovalCommand(approvalId, GetUserId(), request.Approved, request.Reason), ct);
        return NoContent();
    }
    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException("Invalid authenticated user identity.");
    }
}
public sealed record SubmitApprovalRequest(ProfessionalType ProfessionalType);
public sealed record ReviewApprovalRequest(bool Approved, string? Reason);