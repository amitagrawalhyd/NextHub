/*
    NestHub - SQL Server Schema
    ---------------------------
    This script is the DBA-facing source of truth for the NestHub schema. It is kept in
    sync with the EF Core model in NestHub.Infrastructure (validated via
    `dotnet ef migrations script` against the same model) but is hand-formatted here for
    readability and standalone execution.

    Safe to re-run: every object is created only if it does not already exist.
*/

SET NOCOUNT ON;
GO

IF DB_ID(N'NestHub') IS NULL
BEGIN
    CREATE DATABASE [NestHub];
END;
GO

USE [NestHub];
GO

-- Required for spatial indexes and filtered indexes (both used below) — sqlcmd's default is
-- OFF, which fails with "CREATE INDEX failed because ... QUOTED_IDENTIFIER" otherwise.
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================================================
-- Societies
-- =============================================================================
IF OBJECT_ID(N'dbo.Societies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Societies
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Societies PRIMARY KEY,
        Name             NVARCHAR(200)    NOT NULL,
        Address          NVARCHAR(500)    NOT NULL,
        City             NVARCHAR(100)    NOT NULL CONSTRAINT DF_Societies_City DEFAULT (N'Hyderabad'),
        Location         GEOGRAPHY        NULL,
        IsActive         BIT              NOT NULL CONSTRAINT DF_Societies_IsActive DEFAULT (1),
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_Societies_CreatedDateTime DEFAULT (SYSUTCDATETIME())
    );

    CREATE SPATIAL INDEX SIX_Societies_Location ON dbo.Societies (Location) USING GEOGRAPHY_AUTO_GRID;
END;
GO

-- =============================================================================
-- Categories (managed via the Admin "category management matrix")
-- =============================================================================
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        Name      NVARCHAR(100)    NOT NULL,
        IsActive  BIT              NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT (1)
    );

    CREATE UNIQUE INDEX UX_Categories_Name ON dbo.Categories (Name);
END;
GO

-- =============================================================================
-- Users (base identity for Residents, Vendors, and Admins)
-- =============================================================================
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        PhoneNumber      NVARCHAR(20)     NOT NULL,
        Email            NVARCHAR(320)    NULL,
        PasswordHash     NVARCHAR(500)    NOT NULL,
        UserType         NVARCHAR(20)     NOT NULL CONSTRAINT CK_Users_UserType CHECK (UserType IN (N'Resident', N'Vendor', N'Admin')),
        IsVerified       BIT              NOT NULL CONSTRAINT DF_Users_IsVerified DEFAULT (0),
        IsActive         BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_Users_CreatedDateTime DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_Users_PhoneNumber ON dbo.Users (PhoneNumber);
END;
GO

-- =============================================================================
-- Residents
-- =============================================================================
IF OBJECT_ID(N'dbo.Residents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Residents
    (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Residents PRIMARY KEY,
        UserId       UNIQUEIDENTIFIER NOT NULL,
        SocietyId    UNIQUEIDENTIFIER NOT NULL,
        BlockNumber  NVARCHAR(50)     NOT NULL,
        FlatNumber   NVARCHAR(50)     NOT NULL,
        Name         NVARCHAR(200)    NOT NULL,
        CONSTRAINT FK_Residents_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Residents_Societies_SocietyId FOREIGN KEY (SocietyId) REFERENCES dbo.Societies (Id)
    );

    CREATE UNIQUE INDEX UX_Residents_UserId ON dbo.Residents (UserId);
    CREATE INDEX IX_Residents_SocietyId ON dbo.Residents (SocietyId);
END;
GO

-- =============================================================================
-- Vendors
-- =============================================================================
IF OBJECT_ID(N'dbo.Vendors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vendors
    (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Vendors PRIMARY KEY,
        UserId              UNIQUEIDENTIFIER NOT NULL,
        BusinessName        NVARCHAR(200)    NOT NULL,
        LogoUrl             NVARCHAR(1000)   NULL,
        Bio                 NVARCHAR(2000)   NULL,
        WhatsAppNumber      NVARCHAR(20)     NOT NULL,
        OperatingHoursJson  NVARCHAR(MAX)    NOT NULL,
        SubscriptionTier    NVARCHAR(20)     NOT NULL CONSTRAINT CK_Vendors_SubscriptionTier CHECK (SubscriptionTier IN (N'Free', N'Premium')),
        TrustBadgeStatus    NVARCHAR(30)     NOT NULL CONSTRAINT CK_Vendors_TrustBadgeStatus CHECK (TrustBadgeStatus IN (N'None', N'IdVerified', N'SocietyRegular')),
        AverageRating       DECIMAL(3,2)     NOT NULL CONSTRAINT DF_Vendors_AverageRating DEFAULT (0),
        IsApproved          BIT              NOT NULL CONSTRAINT DF_Vendors_IsApproved DEFAULT (0),
        Location            GEOGRAPHY        NULL,
        CONSTRAINT FK_Vendors_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_Vendors_UserId ON dbo.Vendors (UserId);
    CREATE SPATIAL INDEX SIX_Vendors_Location ON dbo.Vendors (Location) USING GEOGRAPHY_AUTO_GRID;
END;
GO

-- =============================================================================
-- Services (owned by a Vendor)
-- =============================================================================
IF OBJECT_ID(N'dbo.Services', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Services
    (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Services PRIMARY KEY,
        VendorId     UNIQUEIDENTIFIER NOT NULL,
        Title        NVARCHAR(200)    NOT NULL,
        Description  NVARCHAR(2000)   NOT NULL,
        PricingJson  NVARCHAR(MAX)    NOT NULL,
        Category     NVARCHAR(100)    NOT NULL,
        CONSTRAINT FK_Services_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_Services_VendorId ON dbo.Services (VendorId);
    CREATE INDEX IX_Services_Category ON dbo.Services (Category);
END;
GO

-- =============================================================================
-- Reviews ("Neighbor-Verified Reviews" - filtered by SocietyId at query time)
-- =============================================================================
IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY,
        ResidentId       UNIQUEIDENTIFIER NOT NULL,
        VendorId         UNIQUEIDENTIFIER NOT NULL,
        SocietyId        UNIQUEIDENTIFIER NOT NULL,
        Rating           INT              NOT NULL CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5),
        Comment          NVARCHAR(2000)   NULL,
        SentimentScore   DECIMAL(3,2)     NULL,
        IsFlagged        BIT              NOT NULL CONSTRAINT DF_Reviews_IsFlagged DEFAULT (0),
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_Reviews_CreatedDateTime DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Reviews_Residents_ResidentId FOREIGN KEY (ResidentId) REFERENCES dbo.Residents (Id),
        CONSTRAINT FK_Reviews_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id),
        CONSTRAINT FK_Reviews_Societies_SocietyId FOREIGN KEY (SocietyId) REFERENCES dbo.Societies (Id)
    );

    CREATE INDEX IX_Reviews_VendorId_SocietyId ON dbo.Reviews (VendorId, SocietyId);
    CREATE INDEX IX_Reviews_ResidentId ON dbo.Reviews (ResidentId);
    CREATE INDEX IX_Reviews_SocietyId ON dbo.Reviews (SocietyId);
END;
GO

-- =============================================================================
-- SosRequests ("SOS Urgent Request" broadcast)
-- =============================================================================
IF OBJECT_ID(N'dbo.SosRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SosRequests
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SosRequests PRIMARY KEY,
        ResidentId       UNIQUEIDENTIFIER NOT NULL,
        SocietyId        UNIQUEIDENTIFIER NOT NULL,
        Category         NVARCHAR(100)    NOT NULL,
        Description      NVARCHAR(2000)   NOT NULL,
        Status           NVARCHAR(20)     NOT NULL CONSTRAINT CK_SosRequests_Status CHECK (Status IN (N'Open', N'Claimed', N'Closed')),
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_SosRequests_CreatedDateTime DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_SosRequests_Residents_ResidentId FOREIGN KEY (ResidentId) REFERENCES dbo.Residents (Id),
        CONSTRAINT FK_SosRequests_Societies_SocietyId FOREIGN KEY (SocietyId) REFERENCES dbo.Societies (Id)
    );

    CREATE INDEX IX_SosRequests_SocietyId_Category_Status ON dbo.SosRequests (SocietyId, Category, Status);
    CREATE INDEX IX_SosRequests_ResidentId ON dbo.SosRequests (ResidentId);
END;
GO

-- =============================================================================
-- SosClaims (a vendor claiming an SosRequest)
-- =============================================================================
IF OBJECT_ID(N'dbo.SosClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SosClaims
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SosClaims PRIMARY KEY,
        SosRequestId     UNIQUEIDENTIFIER NOT NULL,
        VendorId         UNIQUEIDENTIFIER NOT NULL,
        ClaimedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_SosClaims_ClaimedDateTime DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_SosClaims_SosRequests_SosRequestId FOREIGN KEY (SosRequestId) REFERENCES dbo.SosRequests (Id) ON DELETE CASCADE,
        CONSTRAINT FK_SosClaims_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id)
    );

    -- Enforces the "one claim per vendor per request" domain invariant at the data layer too.
    CREATE UNIQUE INDEX UX_SosClaims_SosRequestId_VendorId ON dbo.SosClaims (SosRequestId, VendorId);
    CREATE INDEX IX_SosClaims_VendorId ON dbo.SosClaims (VendorId);
END;
GO

-- =============================================================================
-- AnalyticsLogs (vendor Performance Analytics Dashboard)
-- =============================================================================
IF OBJECT_ID(N'dbo.AnalyticsLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnalyticsLogs
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AnalyticsLogs PRIMARY KEY,
        VendorId         UNIQUEIDENTIFIER NOT NULL,
        ActionType       NVARCHAR(20)     NOT NULL CONSTRAINT CK_AnalyticsLogs_ActionType CHECK (ActionType IN (N'ProfileView', N'CallClick', N'WhatsAppClick')),
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_AnalyticsLogs_CreatedDateTime DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AnalyticsLogs_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id) ON DELETE CASCADE
    );

    -- Composite index supports the dashboard's "events for vendor X within date range" query.
    CREATE INDEX IX_AnalyticsLogs_VendorId_CreatedDateTime ON dbo.AnalyticsLogs (VendorId, CreatedDateTime);
END;
GO

-- =============================================================================
-- Vendors.IsFeatured (Admin "spotlight" flag — featured vendors sort first in search)
-- =============================================================================
IF COL_LENGTH(N'dbo.Vendors', N'IsFeatured') IS NULL
BEGIN
    ALTER TABLE dbo.Vendors ADD IsFeatured BIT NOT NULL CONSTRAINT DF_Vendors_IsFeatured DEFAULT (0);
END;
GO

-- =============================================================================
-- VendorSocietyCoverages (which societies a vendor serves — drives SOS lead matching)
-- =============================================================================
IF OBJECT_ID(N'dbo.VendorSocietyCoverages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VendorSocietyCoverages
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorSocietyCoverages PRIMARY KEY,
        VendorId        UNIQUEIDENTIFIER NOT NULL,
        SocietyId       UNIQUEIDENTIFIER NOT NULL,
        AffiliationType NVARCHAR(20)     NOT NULL CONSTRAINT DF_VendorSocietyCoverages_AffiliationType DEFAULT (N'Manual')
                                          CONSTRAINT CK_VendorSocietyCoverages_AffiliationType CHECK (AffiliationType IN (N'Manual', N'InHouse', N'Nearby')),
        CONSTRAINT FK_VendorSocietyCoverages_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id) ON DELETE CASCADE,
        CONSTRAINT FK_VendorSocietyCoverages_Societies_SocietyId FOREIGN KEY (SocietyId) REFERENCES dbo.Societies (Id)
    );

    CREATE UNIQUE INDEX UX_VendorSocietyCoverages_VendorId_SocietyId ON dbo.VendorSocietyCoverages (VendorId, SocietyId);

    -- At most one InHouse row per vendor (a vendor has at most one "home" society).
    CREATE UNIQUE INDEX UX_VendorSocietyCoverages_Vendor_InHouse ON dbo.VendorSocietyCoverages (VendorId)
        WHERE AffiliationType = N'InHouse';
END;
GO

-- =============================================================================
-- Announcements (Admin-posted per-society notices, shown on the Resident Home screen)
-- =============================================================================
IF OBJECT_ID(N'dbo.Announcements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Announcements
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Announcements PRIMARY KEY,
        SocietyId        UNIQUEIDENTIFIER NOT NULL,
        Title            NVARCHAR(200)    NOT NULL,
        Body             NVARCHAR(2000)   NOT NULL,
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_Announcements_CreatedDateTime DEFAULT (SYSUTCDATETIME()),
        ExpiresAtUtc     DATETIME2        NULL,
        CONSTRAINT FK_Announcements_Societies_SocietyId FOREIGN KEY (SocietyId) REFERENCES dbo.Societies (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_Announcements_SocietyId ON dbo.Announcements (SocietyId);
END;
GO

-- =============================================================================
-- EmergencyContacts (per-society contact directory shown on the Resident SOS screen)
-- =============================================================================
IF OBJECT_ID(N'dbo.EmergencyContacts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmergencyContacts
    (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmergencyContacts PRIMARY KEY,
        SocietyId    UNIQUEIDENTIFIER NOT NULL,
        Name         NVARCHAR(200)    NOT NULL,
        Role         NVARCHAR(100)    NOT NULL,
        PhoneNumber  NVARCHAR(20)     NOT NULL,
        CONSTRAINT FK_EmergencyContacts_Societies_SocietyId FOREIGN KEY (SocietyId) REFERENCES dbo.Societies (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_EmergencyContacts_SocietyId ON dbo.EmergencyContacts (SocietyId);
END;
GO

-- =============================================================================
-- VendorFavorites (a resident bookmarking a vendor)
-- =============================================================================
IF OBJECT_ID(N'dbo.VendorFavorites', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VendorFavorites
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorFavorites PRIMARY KEY,
        ResidentId       UNIQUEIDENTIFIER NOT NULL,
        VendorId         UNIQUEIDENTIFIER NOT NULL,
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_VendorFavorites_CreatedDateTime DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_VendorFavorites_Residents_ResidentId FOREIGN KEY (ResidentId) REFERENCES dbo.Residents (Id) ON DELETE CASCADE,
        CONSTRAINT FK_VendorFavorites_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id)
    );

    CREATE UNIQUE INDEX UX_VendorFavorites_ResidentId_VendorId ON dbo.VendorFavorites (ResidentId, VendorId);
END;
GO

-- =============================================================================
-- VendorBroadcasts (vendor-posted real-time updates/offers, delivered to residents of every
-- society the vendor covers via SignalR)
-- =============================================================================
IF OBJECT_ID(N'dbo.VendorBroadcasts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VendorBroadcasts
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorBroadcasts PRIMARY KEY,
        VendorId         UNIQUEIDENTIFIER NOT NULL,
        Title            NVARCHAR(200)    NOT NULL,
        Message          NVARCHAR(2000)   NOT NULL,
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_VendorBroadcasts_CreatedDateTime DEFAULT (SYSUTCDATETIME()),
        ExpiresAtUtc     DATETIME2        NULL,
        CONSTRAINT FK_VendorBroadcasts_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_VendorBroadcasts_VendorId ON dbo.VendorBroadcasts (VendorId);
END;
GO

-- =============================================================================
-- Users.SocietyId (scopes an Admin account to a single society; NULL = Central Admin)
-- =============================================================================
IF COL_LENGTH(N'dbo.Users', N'SocietyId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD SocietyId UNIQUEIDENTIFIER NULL;
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Societies_SocietyId FOREIGN KEY (SocietyId) REFERENCES dbo.Societies (Id);
    CREATE INDEX IX_Users_SocietyId ON dbo.Users (SocietyId);
END;
GO

-- =============================================================================
-- Demo society-scoped Admin accounts (password: same as the seeded Central Admin)
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE PhoneNumber = N'9000000011')
BEGIN
    INSERT INTO dbo.Users (Id, PhoneNumber, Email, PasswordHash, UserType, IsVerified, IsActive, CreatedDateTime, SocietyId)
    SELECT NEWID(), N'9000000011', N'admin.lakeview@nesthub.example', PasswordHash, N'Admin', 1, 1, SYSUTCDATETIME(),
           (SELECT Id FROM dbo.Societies WHERE Name = N'Lakeview Residency')
    FROM dbo.Users WHERE PhoneNumber = N'9000000001';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE PhoneNumber = N'9000000012')
BEGIN
    INSERT INTO dbo.Users (Id, PhoneNumber, Email, PasswordHash, UserType, IsVerified, IsActive, CreatedDateTime, SocietyId)
    SELECT NEWID(), N'9000000012', N'admin.greenmeadows@nesthub.example', PasswordHash, N'Admin', 1, 1, SYSUTCDATETIME(),
           (SELECT Id FROM dbo.Societies WHERE Name = N'Green Meadows')
    FROM dbo.Users WHERE PhoneNumber = N'9000000001';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE PhoneNumber = N'9000000013')
BEGIN
    INSERT INTO dbo.Users (Id, PhoneNumber, Email, PasswordHash, UserType, IsVerified, IsActive, CreatedDateTime, SocietyId)
    SELECT NEWID(), N'9000000013', N'admin.myhome@nesthub.example', PasswordHash, N'Admin', 1, 1, SYSUTCDATETIME(),
           (SELECT Id FROM dbo.Societies WHERE Name = N'My Home Bhooja')
    FROM dbo.Users WHERE PhoneNumber = N'9000000001';
END;
GO

-- =============================================================================
-- VendorMutes (a resident opting out of a specific vendor's broadcasts/announcements)
-- =============================================================================
IF OBJECT_ID(N'dbo.VendorMutes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VendorMutes
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorMutes PRIMARY KEY,
        ResidentId       UNIQUEIDENTIFIER NOT NULL,
        VendorId         UNIQUEIDENTIFIER NOT NULL,
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_VendorMutes_CreatedDateTime DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_VendorMutes_Residents_ResidentId FOREIGN KEY (ResidentId) REFERENCES dbo.Residents (Id) ON DELETE CASCADE,
        CONSTRAINT FK_VendorMutes_Vendors_VendorId FOREIGN KEY (VendorId) REFERENCES dbo.Vendors (Id)
    );

    CREATE UNIQUE INDEX UX_VendorMutes_ResidentId_VendorId ON dbo.VendorMutes (ResidentId, VendorId);
END;
GO

-- =============================================================================
-- Vendor/Society geography Location + VendorSocietyCoverages.AffiliationType
-- (proximity-aware vendor search: InHouse = explicit affiliation, Nearby = auto-computed
-- from the vendor's own pin against every society's Location, both persisted here instead
-- of recomputed per search. Location replaces the old packed "lat,lng" nvarchar GeoLocation
-- column on Societies with a real spatial type + index.)
-- =============================================================================
IF COL_LENGTH(N'dbo.Societies', N'Location') IS NULL
BEGIN
    ALTER TABLE dbo.Societies ADD Location GEOGRAPHY NULL;

    -- Backfill from the packed "lat,lng" column before dropping it. Wrapped in dynamic SQL
    -- because T-SQL resolves column names in an UPDATE at parse time even inside a guarded IF
    -- block, and GeoLocation no longer exists on a fresh install (or after this block has
    -- already run once) — a bare reference to it would fail to compile in that case.
    IF COL_LENGTH(N'dbo.Societies', N'GeoLocation') IS NOT NULL
    BEGIN
        EXEC(N'
            UPDATE dbo.Societies
            SET Location = geography::Point(
                CAST(LEFT(GeoLocation, CHARINDEX('','', GeoLocation) - 1) AS FLOAT),
                CAST(SUBSTRING(GeoLocation, CHARINDEX('','', GeoLocation) + 1, LEN(GeoLocation)) AS FLOAT),
                4326)
            WHERE GeoLocation IS NOT NULL;
        ');

        ALTER TABLE dbo.Societies DROP COLUMN GeoLocation;
    END;

    CREATE SPATIAL INDEX SIX_Societies_Location ON dbo.Societies (Location) USING GEOGRAPHY_AUTO_GRID;
END;
GO

IF COL_LENGTH(N'dbo.Vendors', N'Location') IS NULL
BEGIN
    ALTER TABLE dbo.Vendors ADD Location GEOGRAPHY NULL;
    CREATE SPATIAL INDEX SIX_Vendors_Location ON dbo.Vendors (Location) USING GEOGRAPHY_AUTO_GRID;
END;
GO

-- Split into separate batches: T-SQL resolves column names in a CHECK constraint / filtered
-- index WHERE clause at compile time for the whole batch, even inside a guarded IF, so a
-- column added earlier in the SAME batch isn't visible yet to a later statement in it.
IF COL_LENGTH(N'dbo.VendorSocietyCoverages', N'AffiliationType') IS NULL
BEGIN
    ALTER TABLE dbo.VendorSocietyCoverages ADD AffiliationType NVARCHAR(20) NOT NULL
        CONSTRAINT DF_VendorSocietyCoverages_AffiliationType DEFAULT (N'Manual');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_VendorSocietyCoverages_AffiliationType')
BEGIN
    ALTER TABLE dbo.VendorSocietyCoverages ADD CONSTRAINT CK_VendorSocietyCoverages_AffiliationType
        CHECK (AffiliationType IN (N'Manual', N'InHouse', N'Nearby'));
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.VendorSocietyCoverages') AND name = N'UX_VendorSocietyCoverages_Vendor_InHouse')
BEGIN
    CREATE UNIQUE INDEX UX_VendorSocietyCoverages_Vendor_InHouse ON dbo.VendorSocietyCoverages (VendorId)
        WHERE AffiliationType = N'InHouse';
END;
GO
