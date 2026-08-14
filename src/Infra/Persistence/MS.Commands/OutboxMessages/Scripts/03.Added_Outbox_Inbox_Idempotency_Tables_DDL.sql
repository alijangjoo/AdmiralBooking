USE AdmiralBooking;
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OutboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OutboxMessages PRIMARY KEY,
        EventType NVARCHAR(128) NOT NULL,
        Payload NVARCHAR(MAX) NOT NULL,
        DeduplicationKey UNIQUEIDENTIFIER NOT NULL,
        Status INT NOT NULL,
        AttemptCount INT NOT NULL,
        LastError NVARCHAR(2000) NULL,
        OccurredAt DATETIMEOFFSET NOT NULL,
        PublishedAt DATETIMEOFFSET NULL,
        NextAttemptAt DATETIMEOFFSET NULL
    );

    CREATE UNIQUE INDEX UX_OutboxMessages_DeduplicationKey ON dbo.OutboxMessages (DeduplicationKey);
    CREATE INDEX IX_OutboxMessages_Pending ON dbo.OutboxMessages (Status, NextAttemptAt);
END
GO

IF OBJECT_ID(N'dbo.InboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InboxMessages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InboxMessages PRIMARY KEY,
        ConsumerName NVARCHAR(128) NOT NULL,
        ProcessedAt DATETIMEOFFSET NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.IdempotencyRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IdempotencyRecords
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_IdempotencyRecords PRIMARY KEY,
        IdempotencyKey NVARCHAR(128) NOT NULL,
        RequestFingerprint NVARCHAR(128) NOT NULL,
        BookingId UNIQUEIDENTIFIER NOT NULL,
        Status INT NOT NULL,
        ResponsePayload NVARCHAR(MAX) NULL,
        ErrorCode NVARCHAR(64) NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        CompletedAt DATETIMEOFFSET NULL
    );

    CREATE UNIQUE INDEX UX_IdempotencyRecords_Key ON dbo.IdempotencyRecords (IdempotencyKey);
END
GO
