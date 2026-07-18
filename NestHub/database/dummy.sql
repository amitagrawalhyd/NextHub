/*
    NestHub - Dummy/Seed Data for Testing
    --------------------------------------
    Run this AFTER Schema.sql (or after `dotnet ef database update`) to populate every table
    with realistic demo data usable end-to-end through NestHub.API — including real, working
    login credentials (all seeded users share one password, hashed with BCrypt below).

    This is the raw-SQL equivalent of NestHub.Infrastructure's DatabaseSeeder (invoked via
    `dotnet run --project src\NestHub.API -- --seed`). Prefer that C# seeder for day-to-day dev
    since it goes through the real domain model; use this script when you need to seed a database
    without running the .NET app at all (e.g. handing data to a DBA, or a SQL-only CI step).

    Login password for every seeded user: Passw0rd!23
    Safe to re-run: every INSERT is guarded by a NOT EXISTS check against the fixed GUIDs below.
*/

USE [NestHub];
GO

DECLARE @PasswordHash NVARCHAR(500) = N'$2a$11$dBLOxl5tMHmUCox94BMXsuMrtEDx.i6efbURS/qUFjceo/y3TmTJK'; -- BCrypt hash of "Passw0rd!23"
DECLARE @AlwaysOpenJson NVARCHAR(MAX) = N'{"Sunday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Monday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Tuesday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Wednesday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Thursday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Friday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Saturday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false}}';

-- =============================================================================
-- Societies
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM Societies WHERE Id = '11111111-1111-1111-1111-111111111001')
BEGIN
    INSERT INTO Societies (Id, Name, Address, City, GeoLocation, IsActive, CreatedDateTime) VALUES
    ('11111111-1111-1111-1111-111111111001', N'Lakeview Residency', N'Gachibowli, Hyderabad', N'Hyderabad', '17.4448,78.3498', 1, SYSUTCDATETIME()),
    ('11111111-1111-1111-1111-111111111002', N'Green Meadows',      N'Kondapur, Hyderabad',   N'Hyderabad', '17.4615,78.363',  1, SYSUTCDATETIME()),
    ('11111111-1111-1111-1111-111111111003', N'My Home Bhooja',     N'Kokapet, Hyderabad',    N'Hyderabad', '17.4045,78.321',  1, SYSUTCDATETIME());
END;
GO

-- =============================================================================
-- Categories
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Plumbing')
BEGIN
    INSERT INTO Categories (Id, Name, IsActive) VALUES
    (NEWID(), N'Plumbing', 1),
    (NEWID(), N'Electrical', 1),
    (NEWID(), N'Home Maintenance', 1),
    (NEWID(), N'Tutors', 1),
    (NEWID(), N'Food & Catering', 1),
    (NEWID(), N'Health & Wellness', 1),
    (NEWID(), N'Pest Control', 1),
    (NEWID(), N'Appliance Repair', 1);
END;
GO

-- =============================================================================
-- Users: Admin
-- =============================================================================
DECLARE @PasswordHash NVARCHAR(500) = N'$2a$11$dBLOxl5tMHmUCox94BMXsuMrtEDx.i6efbURS/qUFjceo/y3TmTJK';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = '22222222-2222-2222-2222-222222222001')
BEGIN
    INSERT INTO Users (Id, PhoneNumber, Email, PasswordHash, UserType, IsVerified, IsActive, CreatedDateTime) VALUES
    ('22222222-2222-2222-2222-222222222001', N'9000000001', N'admin@nesthub.example', @PasswordHash, N'Admin', 1, 1, SYSUTCDATETIME());
END;
GO

-- =============================================================================
-- Users + Residents (Lakeview: Amit, Priya | Green Meadows: Ravi, Sneha | My Home: Arjun)
-- =============================================================================
DECLARE @PasswordHash NVARCHAR(500) = N'$2a$11$dBLOxl5tMHmUCox94BMXsuMrtEDx.i6efbURS/qUFjceo/y3TmTJK';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = '22222222-2222-2222-2222-222222222101')
BEGIN
    INSERT INTO Users (Id, PhoneNumber, Email, PasswordHash, UserType, IsVerified, IsActive, CreatedDateTime) VALUES
    ('22222222-2222-2222-2222-222222222101', N'9000000101', NULL, @PasswordHash, N'Resident', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222102', N'9000000102', NULL, @PasswordHash, N'Resident', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222103', N'9000000103', NULL, @PasswordHash, N'Resident', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222104', N'9000000104', NULL, @PasswordHash, N'Resident', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222105', N'9000000105', NULL, @PasswordHash, N'Resident', 1, 1, SYSUTCDATETIME());

    INSERT INTO Residents (Id, UserId, SocietyId, BlockNumber, FlatNumber, Name) VALUES
    ('33333333-3333-3333-3333-333333333101', '22222222-2222-2222-2222-222222222101', '11111111-1111-1111-1111-111111111001', N'A', N'204', N'Amit Agrawal'),
    ('33333333-3333-3333-3333-333333333102', '22222222-2222-2222-2222-222222222102', '11111111-1111-1111-1111-111111111001', N'B', N'301', N'Priya Sharma'),
    ('33333333-3333-3333-3333-333333333103', '22222222-2222-2222-2222-222222222103', '11111111-1111-1111-1111-111111111002', N'C', N'102', N'Ravi Kumar'),
    ('33333333-3333-3333-3333-333333333104', '22222222-2222-2222-2222-222222222104', '11111111-1111-1111-1111-111111111002', N'A', N'405', N'Sneha Reddy'),
    ('33333333-3333-3333-3333-333333333105', '22222222-2222-2222-2222-222222222105', '11111111-1111-1111-1111-111111111003', N'D', N'701', N'Arjun Rao');
END;
GO

-- =============================================================================
-- Users + Vendors + Services
-- =============================================================================
DECLARE @PasswordHash NVARCHAR(500) = N'$2a$11$dBLOxl5tMHmUCox94BMXsuMrtEDx.i6efbURS/qUFjceo/y3TmTJK';
DECLARE @AlwaysOpenJson NVARCHAR(MAX) = N'{"Sunday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Monday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Tuesday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Wednesday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Thursday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Friday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false},"Saturday":{"OpensAt":"00:00:00","ClosesAt":"23:59:59.9999999","IsClosed":false}}';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = '22222222-2222-2222-2222-222222222201')
BEGIN
    INSERT INTO Users (Id, PhoneNumber, Email, PasswordHash, UserType, IsVerified, IsActive, CreatedDateTime) VALUES
    ('22222222-2222-2222-2222-222222222201', N'9000000201', NULL, @PasswordHash, N'Vendor', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222202', N'9000000202', NULL, @PasswordHash, N'Vendor', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222203', N'9000000203', NULL, @PasswordHash, N'Vendor', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222204', N'9000000204', NULL, @PasswordHash, N'Vendor', 1, 1, SYSUTCDATETIME()),
    ('22222222-2222-2222-2222-222222222205', N'9000000205', NULL, @PasswordHash, N'Vendor', 1, 1, SYSUTCDATETIME());

    -- Approved vendors (Sharma Plumbing = Premium + IdVerified; BrightSpark = Free + SocietyRegular; GreenLeaf, MathGenius = Free/None)
    INSERT INTO Vendors (Id, UserId, BusinessName, LogoUrl, Bio, WhatsAppNumber, OperatingHoursJson, SubscriptionTier, TrustBadgeStatus, AverageRating, IsApproved) VALUES
    ('44444444-4444-4444-4444-444444444201', '22222222-2222-2222-2222-222222222201', N'Sharma Plumbing Works',    NULL, N'24x7 reliable plumbing services across Hyderabad.',       N'9876543210', @AlwaysOpenJson, N'Premium', N'IdVerified',     4.5, 1),
    ('44444444-4444-4444-4444-444444444202', '22222222-2222-2222-2222-222222222202', N'BrightSpark Electricians', NULL, N'Licensed electricians for home and society wiring.',     N'9876543211', @AlwaysOpenJson, N'Free',    N'SocietyRegular', 5.0, 1),
    ('44444444-4444-4444-4444-444444444203', '22222222-2222-2222-2222-222222222203', N'GreenLeaf Pest Control',   NULL, N'Eco-friendly pest control for apartments.',              N'9876543212', @AlwaysOpenJson, N'Free',    N'None',           0.0, 1),
    ('44444444-4444-4444-4444-444444444204', '22222222-2222-2222-2222-222222222204', N'MathGenius Tutoring',      NULL, N'Home tuition for grades 6-12, Math and Science.',        N'9876543213', @AlwaysOpenJson, N'Premium', N'IdVerified',     5.0, 1),
    -- Pending approval — use this one to test the Admin "Vendor Onboarding Queue"
    ('44444444-4444-4444-4444-444444444205', '22222222-2222-2222-2222-222222222205', N'QuickFix Appliance Repair', NULL, N'Awaiting approval — new to NestHub.',                   N'9876543214', @AlwaysOpenJson, N'Free',    N'None',           0.0, 0);

    INSERT INTO Services (Id, VendorId, Title, Description, PricingJson, Category) VALUES
    ('55555555-5555-5555-5555-555555555201', '44444444-4444-4444-4444-444444444201', N'Tap & Pipe Repair', N'Fix leaking taps and pipes.',      N'{"Type":3,"Amount":300,"Currency":"INR"}',   N'Plumbing'),
    ('55555555-5555-5555-5555-555555555202', '44444444-4444-4444-4444-444444444201', N'Bathroom Fitting',  N'Full bathroom fixture installation.', N'{"Type":4,"Amount":null,"Currency":"INR"}', N'Plumbing'),
    ('55555555-5555-5555-5555-555555555203', '44444444-4444-4444-4444-444444444202', N'Wiring Repair',     N'Fix faulty wiring and switches.',   N'{"Type":2,"Amount":250,"Currency":"INR"}',   N'Electrical'),
    ('55555555-5555-5555-5555-555555555204', '44444444-4444-4444-4444-444444444203', N'General Pest Treatment', N'Cockroach and ant treatment.', N'{"Type":1,"Amount":1200,"Currency":"INR"}',  N'Pest Control'),
    ('55555555-5555-5555-5555-555555555205', '44444444-4444-4444-4444-444444444204', N'Math Tuition',      N'One-on-one math coaching.',        N'{"Type":2,"Amount":500,"Currency":"INR"}',   N'Tutors');
END;
GO

-- =============================================================================
-- Reviews (demonstrates Neighbor-Verified filtering: same vendor, different societies)
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM Reviews WHERE Id = '66666666-6666-6666-6666-666666666601')
BEGIN
    INSERT INTO Reviews (Id, ResidentId, VendorId, SocietyId, Rating, Comment, SentimentScore, IsFlagged, CreatedDateTime) VALUES
    ('66666666-6666-6666-6666-666666666601', '33333333-3333-3333-3333-333333333101', '44444444-4444-4444-4444-444444444201', '11111111-1111-1111-1111-111111111001', 5, N'Excellent and prompt service, highly recommend!',   1.0,  0, SYSUTCDATETIME()),
    ('66666666-6666-6666-6666-666666666602', '33333333-3333-3333-3333-333333333102', '44444444-4444-4444-4444-444444444201', '11111111-1111-1111-1111-111111111001', 4, N'Good work, arrived a bit late but fixed the issue.', 0.0,  0, SYSUTCDATETIME()),
    ('66666666-6666-6666-6666-666666666603', '33333333-3333-3333-3333-333333333103', '44444444-4444-4444-4444-444444444201', '11111111-1111-1111-1111-111111111002', 2, N'Overcharged compared to the quote given.',           -0.5, 0, SYSUTCDATETIME()),
    ('66666666-6666-6666-6666-666666666604', '33333333-3333-3333-3333-333333333101', '44444444-4444-4444-4444-444444444202', '11111111-1111-1111-1111-111111111001', 5, N'Very professional and quick.',                       1.0,  0, SYSUTCDATETIME()),
    ('66666666-6666-6666-6666-666666666605', '33333333-3333-3333-3333-333333333102', '44444444-4444-4444-4444-444444444204', '11111111-1111-1111-1111-111111111001', 5, N'My daughter''s grades improved a lot, wonderful tutor.', 1.0, 0, SYSUTCDATETIME()),
    -- Flagged — use this one to test Admin "Review Moderation"
    ('66666666-6666-6666-6666-666666666606', '33333333-3333-3333-3333-333333333104', '44444444-4444-4444-4444-444444444201', '11111111-1111-1111-1111-111111111002', 1, N'Rude behavior, would not recommend at all.',        -1.0, 1, SYSUTCDATETIME());
END;
GO

-- =============================================================================
-- SOS Requests + Claims
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM SosRequests WHERE Id = '77777777-7777-7777-7777-777777777701')
BEGIN
    INSERT INTO SosRequests (Id, ResidentId, SocietyId, Category, Description, Status, CreatedDateTime) VALUES
    ('77777777-7777-7777-7777-777777777701', '33333333-3333-3333-3333-333333333103', '11111111-1111-1111-1111-111111111002', N'Plumbing',   N'Leaking pipe in kitchen, urgent!',            N'Open',    SYSUTCDATETIME()),
    ('77777777-7777-7777-7777-777777777702', '33333333-3333-3333-3333-333333333101', '11111111-1111-1111-1111-111111111001', N'Electrical', N'Power socket sparking in living room!',       N'Claimed', SYSUTCDATETIME());

    INSERT INTO SosClaims (Id, SosRequestId, VendorId, ClaimedDateTime) VALUES
    ('88888888-8888-8888-8888-888888888801', '77777777-7777-7777-7777-777777777702', '44444444-4444-4444-4444-444444444202', SYSUTCDATETIME());
END;
GO

-- =============================================================================
-- Analytics Logs — spread across the last 14 days so date-range dashboard queries have
-- something meaningful to aggregate (unlike a same-instant batch insert).
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM AnalyticsLogs WHERE VendorId = '44444444-4444-4444-4444-444444444201')
BEGIN
    ;WITH Numbers AS (
        SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
        FROM sys.all_objects
    ),
    ApprovedVendors AS (
        SELECT Id FROM Vendors WHERE IsApproved = 1
    )
    INSERT INTO AnalyticsLogs (Id, VendorId, ActionType, CreatedDateTime)
    SELECT
        NEWID(),
        v.Id,
        CASE ABS(CHECKSUM(NEWID())) % 3
            WHEN 0 THEN N'ProfileView'
            WHEN 1 THEN N'CallClick'
            ELSE N'WhatsAppClick'
        END,
        DATEADD(DAY, -1 * (ABS(CHECKSUM(NEWID())) % 14), DATEADD(SECOND, -1 * (ABS(CHECKSUM(NEWID())) % 86400), SYSUTCDATETIME()))
    FROM ApprovedVendors v
    CROSS JOIN Numbers n;
END;
GO
