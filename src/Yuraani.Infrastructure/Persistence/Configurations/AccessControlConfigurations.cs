using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuraani.Domain.Identity;
namespace Yuraani.Infrastructure.Persistence.Configurations;
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles"); b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(80).IsRequired();
        b.Property(x => x.Description).HasMaxLength(250);
        b.HasIndex(x => x.Name).IsUnique();
    }
}
public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("Permissions"); b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(120).IsRequired();
        b.Property(x => x.Description).HasMaxLength(250).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
    }
}
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("UserRoles"); b.HasKey(x => new { x.UserAccountId, x.RoleId });
        b.Property(x => x.AssignedAt).HasColumnType("datetime2");
        b.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.AssignedByUserAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("RolePermissions"); b.HasKey(x => new { x.RoleId, x.PermissionId });
        b.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Permission>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(100).IsRequired();
        b.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(100);
        b.Property(x => x.Details).HasMaxLength(2000);
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.Property(x => x.OccurredAt).HasColumnType("datetime2");
        b.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt });
        b.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.ActorUserAccountId).OnDelete(DeleteBehavior.SetNull);
    }
}