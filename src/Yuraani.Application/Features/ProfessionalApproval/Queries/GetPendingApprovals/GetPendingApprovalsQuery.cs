using MediatR;
namespace Yuraani.Application.Features.ProfessionalApproval.Queries.GetPendingApprovals;

/// <summary>DTO بيانات طلب الاعتماد المعلّق.</summary>
public sealed record PendingApprovalDto(
    Guid ApprovalId,
    Guid ProviderId,
    string ProfessionalType,
    byte AttemptNumber,
    DateTimeOffset SubmittedAt);

/// <summary>استعلام جلب طلبات الاعتماد المعلّقة.</summary>
public sealed record GetPendingApprovalsQuery : IRequest<IReadOnlyList<PendingApprovalDto>>;
