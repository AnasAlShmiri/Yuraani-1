/*
  Apply after reviewing against the target SQL Server database.
  Role assignment is intentionally not self-service: provision the first administrator
  through a trusted deployment procedure, then manage assignments through audited admin tooling.
*/
CREATE TABLE dbo.Roles (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Name nvarchar(80) NOT NULL,
    Description nvarchar(250) NULL,
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
CREATE TABLE dbo.Permissions (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Permissions PRIMARY KEY,
    Code nvarchar(120) NOT NULL,
    Description nvarchar(250) NOT NULL,
    CONSTRAINT UQ_Permissions_Code UNIQUE (Code)
);
CREATE TABLE dbo.UserRoles (
    UserAccountId uniqueidentifier NOT NULL,
    RoleId uniqueidentifier NOT NULL,
    AssignedAt datetime2 NOT NULL,
    AssignedByUserAccountId uniqueidentifier NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserAccountId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserAccountId) REFERENCES dbo.UserAccounts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id),
    CONSTRAINT FK_UserRoles_AssignedBy FOREIGN KEY (AssignedByUserAccountId) REFERENCES dbo.UserAccounts(Id)
);
CREATE TABLE dbo.RolePermissions (
    RoleId uniqueidentifier NOT NULL,
    PermissionId uniqueidentifier NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id)
);
CREATE TABLE dbo.AuditLogs (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    ActorUserAccountId uniqueidentifier NULL,
    Action nvarchar(100) NOT NULL,
    EntityType nvarchar(100) NOT NULL,
    EntityId nvarchar(100) NULL,
    Details nvarchar(2000) NULL,
    IpAddress nvarchar(64) NULL,
    OccurredAt datetime2 NOT NULL,
    CONSTRAINT FK_AuditLogs_Actor FOREIGN KEY (ActorUserAccountId) REFERENCES dbo.UserAccounts(Id) ON DELETE SET NULL
);
CREATE INDEX IX_AuditLogs_Entity ON dbo.AuditLogs(EntityType, EntityId, OccurredAt);
