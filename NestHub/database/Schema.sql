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
        GeoLocation      NVARCHAR(64)     NULL,
        IsActive         BIT              NOT NULL CONSTRAINT DF_Societies_IsActive DEFAULT (1),
        CreatedDateTime  DATETIME2        NOT NULL CONSTRAINT DF_Societies_CreatedDateTime DEFAULT (SYSUTCDATETIME())
    );
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
        CONSTRAINT FK_Vendors_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_Vendors_UserId ON dbo.Vendors (UserId);
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
