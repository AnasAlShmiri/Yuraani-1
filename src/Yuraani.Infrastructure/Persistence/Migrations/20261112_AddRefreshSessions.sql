CREATE TABLE dbo.UserSessions (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_UserSessions PRIMARY KEY,
    UserAccountId uniqueidentifier NOT NULL,
    TokenHash nvarchar(64) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    ExpiresAt datetime2 NOT NULL,
    RevokedAt datetime2 NULL,
    ReplacedBySessionId uniqueidentifier NULL,
    IpAddress nvarchar(64) NULL,
    UserAgent nvarchar(512) NULL,
    RowVersion rowversion NOT NULL,
    CONSTRAINT FK_UserSessions_User FOREIGN KEY (UserAccountId) REFERENCES dbo.UserAccounts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserSessions_Replacement FOREIGN KEY (ReplacedBySessionId) REFERENCES dbo.UserSessions(Id)
);
CREATE UNIQUE INDEX UX_UserSessions_TokenHash ON dbo.UserSessions(TokenHash);
CREATE INDEX IX_UserSessions_User_Expiry ON dbo.UserSessions(UserAccountId, ExpiresAt);