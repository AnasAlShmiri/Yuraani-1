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
        var providerId = Guid.NewGuid();
        var doctor = ProfessionalApproval.Submit(providerId, ProfessionalType.Doctor, 1, now);
        var nurse = ProfessionalApproval.Submit(providerId, ProfessionalType.Nurse, 1, now);

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
            request.Decide(Guid.NewGuid(), approved: false, reason: " ", now: DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Decide_ShouldPreventSecondDecision()
    {
        var request = ProfessionalApproval.Submit(Guid.NewGuid(), ProfessionalType.Doctor, 1, DateTimeOffset.UtcNow);
        request.Decide(Guid.NewGuid(), approved: true, reason: null, now: DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            request.Decide(Guid.NewGuid(), approved: false, reason: "رفض", now: DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ExpiredLicense_ShouldNotBeApproved()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var license = ProfessionalLicense.Submit(Guid.NewGuid(), ProfessionalType.Doctor,
            "DOC-1", today.AddYears(-1), today.AddDays(-1), "private/doc-1");

        Assert.Throws<InvalidOperationException>(() => license.Review(true, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void LicenseExpiry_ShouldBeScopedToItsProfessionalType()
    {
        var today = new DateOnly(2026, 11, 12);
        var doctor = ProfessionalLicense.Submit(Guid.NewGuid(), ProfessionalType.Doctor,
            "DOC-1", today.AddYears(-1), today.AddDays(-1), "private/doc-1");
        Assert.Throws<InvalidOperationException>(() => doctor.Review(true, DateTimeOffset.UtcNow));
        Assert.Equal(LicenseReviewStatus.Pending, doctor.ReviewStatus);
    }

    [Fact]
    public void ReviewHistory_ShouldPreserveEachActionAsSeparateEntry()
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
