using MediatR;
using Yuraani.Application.Features.ProfessionalApproval.Abstractions;
using Yuraani.Domain.Identity;
using Yuraani.Domain.ProfessionalApproval;
using ApprovalRequest = Yuraani.Domain.ProfessionalApproval.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Commands.SubmitProfessionalApproval;
public sealed class SubmitProfessionalApprovalHandler(IProfessionalApprovalRepository repository, TimeProvider clock)
    : IRequestHandler<SubmitProfessionalApprovalCommand, Guid>
{
    public async Task<Guid> Handle(SubmitProfessionalApprovalCommand request, CancellationToken ct)
    {
        var providerId = await repository.FindProviderIdByAccountIdAsync(request.UserAccountId, ct)
            ?? throw new UnauthorizedAccessException("A CareProvider profile is required.");
        if (await repository.HasOpenOrApprovedRequestAsync(providerId, request.ProfessionalType, ct))
            throw new InvalidOperationException("A pending or approved request already exists for this professional type.");
        var previousAttempt = await repository.GetLatestAttemptAsync(providerId, request.ProfessionalType, ct);
        if (previousAttempt >= 3) throw new InvalidOperationException("The maximum of three submissions has been reached for this professional type.");
        var now = clock.GetUtcNow();
        var approval = ApprovalRequest.Submit(providerId, request.ProfessionalType, checked((byte)(previousAttempt + 1)), now);
        await repository.AddApprovalAsync(approval, ct);
        await repository.AddAuditLogAsync(AuditLog.Record(request.UserAccountId, "ProfessionalApproval.Submitted",
            "ProfessionalApproval", approval.Id.ToString(), request.ProfessionalType.ToString(), null, now), ct);
        await repository.SaveChangesAsync(ct);
        return approval.Id;
    }
}