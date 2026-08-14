USE AdmiralBooking;
GO

IF OBJECT_ID(N'dbo.Voyages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Voyages
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Voyages PRIMARY KEY,
        Origin NVARCHAR(128) NOT NULL,
        Destination NVARCHAR(128) NOT NULL,
        DepartureDate DATETIMEOFFSET NOT NULL,
        TotalCapacity INT NOT NULL,
        ReservedCapacity INT NOT NULL,
        Status INT NOT NULL,
        Version INT NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT CK_Voyages_ReservedCapacity CHECK (ReservedCapacity <= TotalCapacity),
        CONSTRAINT CK_Voyages_TotalCapacity CHECK (TotalCapacity > 0)
    );
END
GO

IF OBJECT_ID(N'dbo.CapacityReservations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CapacityReservations
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CapacityReservations PRIMARY KEY,
        VoyageId UNIQUEIDENTIFIER NOT NULL,
        BookingId UNIQUEIDENTIFIER NOT NULL,
        ReservedUnits INT NOT NULL,
        Status INT NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT FK_CapacityReservations_Voyages FOREIGN KEY (VoyageId) REFERENCES dbo.Voyages (Id)
    );

    CREATE UNIQUE INDEX UX_CapacityReservations_Booking_Voyage_Active
        ON dbo.CapacityReservations (BookingId, VoyageId)
        WHERE Status = 1;
END
GO
