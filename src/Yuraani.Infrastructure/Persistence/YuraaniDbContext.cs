using Microsoft.EntityFrameworkCore;
using Yuraani.Domain.Identity;
using Yuraani.Domain.ProfessionalApproval;

namespace Yuraani.Infrastructure.Persistence;

public sealed class YuraaniDbContext(DbContextOptions<YuraaniDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<VerificationChallenge> VerificationChallenges => Set<VerificationChallenge>();
    public DbSet<LoginHistory> LoginHistory => Set<LoginHistory>();

    public DbSet<CareProvider> CareProviders => Set<CareProvider>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<ProfessionalApproval> ProfessionalApprovals => Set<ProfessionalApproval>();
    public DbSet<ProfessionalApprovalReview> ProfessionalApprovalReviews => Set<ProfessionalApprovalReview>();
    public DbSet<ProfessionalLicense> ProfessionalLicenses => Set<ProfessionalLicense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(YuraaniDbContext).Assembly);
    }
}
