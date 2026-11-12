using Xunit;
using Yuraani.Domain.ProfessionalApproval;

namespace Yuraani.UnitTests.ProfessionalApproval;

public sealed class ProfessionalApprovalTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Submit_ShouldRejectAttemptOutsideOneToThree(byte attempt)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProfessionalApproval.Submit(Guid.NewGuid(), ProfessionalType.Doctor, attempt, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Submit_ShouldStartPendingAndKeepProfessionIndependent()
    {
        var now = DateTimeOffset.UtcNow;
        var doctor = ProfessionalApproval.Submit(Guid.NewGuid(), ProfessionalType.Doctor, 1, now);
        var nurse = ProfessionalApproval.Submit(doctor.ProviderId, ProfessionalType.Nurse, 1, now);

        Assert.Equal(ApprovalStatus.Pending, doctor.ApprovalStatus);
        Assert.Equal(ProfessionalType.Doctor, doctor.ProfessionalType);
        Assert.Equal(ProfessionalType.Nurse, nurse.ProfessionalType);
        Assert.NotEqual(doctor.Id, nurse.Id);
    }

    [Fact]
    public void Decide_ShouldRequireReasonWhenRejected()
    {
        var request = ProfessionalApproval.Submit(Guid.NewGuid(), ProfessionalType.Nurse, 1, DateTimeOffset.UtcNow);
        Assert.Throws<ArgumentException>(() =>
            request.Decide(Guid.NewGuid(), approved: false, reason: " ", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Decide_ShouldPreventSecondDecision()
    {
        var request = ProfessionalApproval.Submit(Guid.NewGuid(), ProfessionalType.Doctor, 1, DateTimeOffset.UtcNow);
        request.Decide(Guid.NewGuid(), approved: true, reason: null, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            request.Decide(Guid.NewGuid(), approved: false, reason: "رفض", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void LicenseExpiry_ShouldNotAffectOtherProfessionalType()
    {
        var today = new DateOnly(2026, 11, 12);
        var doctor = ProfessionalLicense.Submit(Guid.NewGuid(), ProfessionalType.Doctor,
            "DOC-1", today.AddYears(-1), today.AddDays(-1), "private/doc-1");
        doctor.Review(true, DateTimeOffset.UtcNow);
        doctor.MarkExpired(today);

        Assert.Equal(LicenseReviewStatus.Expired, doctor.ReviewStatus);
        Assert.False(doctor.IsCurrent);
    }

    [Fact]
    public void ReviewHistory_ShouldBeAppendableWithoutChangingEarlierEntry()
    {
        var approvalId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var submitted = ProfessionalApprovalReview.Record(
            approvalId, adminId, ApprovalReviewAction.Submitted, null, DateTimeOffset.UtcNow);
        var rejected = ProfessionalApprovalReview.Record(
            approvalId, adminId, ApprovalReviewAction.Rejected, "نقص مستند", DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.Equal(ApprovalReviewAction.Submitted, submitted.ReviewAction);
        Assert.Equal(ApprovalReviewAction.Rejected, rejected.ReviewAction);
        Assert.NotEqual(submitted.Id, rejected.Id);
    }
}
