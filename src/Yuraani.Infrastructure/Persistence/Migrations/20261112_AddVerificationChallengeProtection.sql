/*
    يضيف عداد المحاولات ووقت إبطال رمز التحقق.
    نفّذ هذا الملف مرة واحدة على قاعدة بيانات المشروع قبل تشغيل النسخة الجديدة.
*/
IF COL_LENGTH('dbo.VerificationChallenges', 'AttemptCount') IS NULL
BEGIN
    ALTER TABLE dbo.VerificationChallenges
    ADD AttemptCount INT NOT NULL
        CONSTRAINT DF_VerificationChallenges_AttemptCount DEFAULT (0);
END;
GO

IF COL_LENGTH('dbo.VerificationChallenges', 'RevokedAt') IS NULL
BEGIN
    ALTER TABLE dbo.VerificationChallenges
    ADD RevokedAt DATETIMEOFFSET NULL;
END;
GO
