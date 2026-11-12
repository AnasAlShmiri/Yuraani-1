using MediatR;
using Yuraani.Application.Features.ProfessionalApproval.Abstractions;
using Yuraani.Domain.ProfessionalApproval;
namespace Yuraani.Application.Features.ProfessionalApproval.Commands.ReviewProfessionalApproval;
public sealed class ReviewProfessionalApprovalHandler(IProfessionalApprovalRepository repository, TimeProvider clock)
    : IRequestHandler<ReviewProfessionalApprovalCommand>
{
    public async Task Handle(ReviewProfessionalApprovalCommand request, CancellationToken ct)
    {
        var adminId = await repository.FindAdministratorIdByAccountIdAsync(request.AdministratorAccountId, ct)
            ?? throw new UnauthorizedAccessException("An Administrator profile is required.");
        var approval = await repository.FindApprovalAsync(request.ApprovalId, ct)
            ?? throw new KeyNotFoundException("Professional approval request was not found.");
        var now = clock.GetUtcNow();
        approval.Decide(adminId, request.Approved, request.Reason, now);
        var action = request.Approved ? ApprovalReviewAction.Approved : ApprovalReviewAction.Rejected;
        await repository.AddReviewAsync(ProfessionalApprovalReview.Record(approval.Id, adminId, action, request.Reason, now), ct);
        await repository.SaveChangesAsync(ct);
    }
}