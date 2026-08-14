USE AdmiralBooking;
GO

IF OBJECT_ID(N'dbo.Bookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bookings
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        VoyageId UNIQUEIDENTIFIER NOT NULL,
        RequestedUnits INT NOT NULL,
        Status INT NOT NULL,
        CapacityReservationId UNIQUEIDENTIFIER NULL,
        ConfirmedAt DATETIMEOFFSET NULL,
        Version INT NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT CK_Bookings_RequestedUnits CHECK (RequestedUnits > 0)
    );

    CREATE INDEX IX_Bookings_VoyageId ON dbo.Bookings (VoyageId);
END
GO
