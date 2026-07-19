using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Analytics;
using NestHub.Domain.Categories;
using NestHub.Domain.Enums;
using NestHub.Domain.Reviews;
using NestHub.Domain.Societies;
using NestHub.Domain.SosRequests;
using NestHub.Domain.Users;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Seed;

/// <summary>
/// Populates every table with realistic demo data for manual/QA testing. Safe to run multiple
/// times — it no-ops if any societies already exist. Invoked via `dotnet run -- --seed`.
/// </summary>
public static class DatabaseSeeder
{
    public const string DemoPassword = "Passw0rd!23";

    public static async Task SeedAsync(NestHubDbContext context, IPasswordHasher passwordHasher, IAiService aiService)
    {
        if (context.Societies.Any())
            return;

        var passwordHash = passwordHasher.Hash(DemoPassword);

        // ---- Societies ----
        var lakeview = Society.Register("Lakeview Residency", "Gachibowli, Hyderabad", GeoLocation.Create(17.4448, 78.3498));
        var greenMeadows = Society.Register("Green Meadows", "Kondapur, Hyderabad", GeoLocation.Create(17.4615, 78.3630));
        var myHome = Society.Register("My Home Bhooja", "Kokapet, Hyderabad", GeoLocation.Create(17.4045, 78.3210));
        context.Societies.AddRange(lakeview, greenMeadows, myHome);

        // ---- Categories ----
        var categoryNames = new[] { "Plumbing", "Electrical", "Home Maintenance", "Tutors", "Food & Catering", "Health & Wellness", "Pest Control", "Appliance Repair" };
        foreach (var name in categoryNames)
            context.Categories.Add(Category.Create(name));

        // ---- Admin (Central — unrestricted access across every society) ----
        var admin = User.Register(PhoneNumber.Create("9000000001"), Email.Create("admin@nesthub.example"), passwordHash, UserType.Admin);
        admin.MarkVerified();
        context.Users.Add(admin);

        // ---- Society Admins (each scoped to manage only their own society) ----
        var societyAdminSeeds = new (string Phone, string Email, Society Society)[]
        {
            ("9000000011", "admin.lakeview@nesthub.example", lakeview),
            ("9000000012", "admin.greenmeadows@nesthub.example", greenMeadows),
            ("9000000013", "admin.myhome@nesthub.example", myHome),
        };
        foreach (var (phone, email, society) in societyAdminSeeds)
        {
            var societyAdmin = User.Register(PhoneNumber.Create(phone), Email.Create(email), passwordHash, UserType.Admin, society.Id);
            societyAdmin.MarkVerified();
            context.Users.Add(societyAdmin);
        }

        // ---- Residents (spread across societies) ----
        var residentSeeds = new (string Phone, string Name, Society Society, string Block, string Flat)[]
        {
            ("9000000101", "Amit Agrawal", lakeview, "A", "204"),
            ("9000000102", "Priya Sharma", lakeview, "B", "301"),
            ("9000000103", "Ravi Kumar", greenMeadows, "C", "102"),
            ("9000000104", "Sneha Reddy", greenMeadows, "A", "405"),
            ("9000000105", "Arjun Rao", myHome, "D", "701"),
        };

        var residents = new List<(User User, Domain.Residents.Resident Resident)>();
        foreach (var (phone, name, society, block, flat) in residentSeeds)
        {
            var user = User.Register(PhoneNumber.Create(phone), null, passwordHash, UserType.Resident);
            user.MarkVerified();
            var resident = Domain.Residents.Resident.Create(user.Id, society.Id, block, flat, name);
            context.Users.Add(user);
            context.Residents.Add(resident);
            residents.Add((user, resident));
        }

        // ---- Vendors ----
        // Latitude/longitude chosen to demonstrate every proximity tier out of the box:
        // Sharma/GreenLeaf sit exactly on their home society's pin (InHouse, set explicitly
        // below); BrightSpark/MathGenius sit a few km from a society (auto-Nearby, computed
        // the same way RecomputeVendorProximityCommand would); QuickFix sits ~27km from every
        // society (Other/far — fittingly, also the one vendor still pending approval).
        var vendorSeeds = new (string Phone, string Business, string WhatsApp, string Bio, bool Approved, SubscriptionTier Tier, TrustBadgeStatus Badge, double? Latitude, double? Longitude, (string Title, string Desc, string Category, PricingType PType, decimal? Amount)[] Services)[]
        {
            ("9000000201", "Sharma Plumbing Works", "9876543210", "24x7 reliable plumbing services across Hyderabad.", true, SubscriptionTier.Premium, TrustBadgeStatus.IdVerified, 17.4448, 78.3498,
                new[] { ("Tap & Pipe Repair", "Fix leaking taps and pipes.", "Plumbing", PricingType.StartingFrom, (decimal?)300), ("Bathroom Fitting", "Full bathroom fixture installation.", "Plumbing", PricingType.ContactForQuote, (decimal?)null) }),
            ("9000000202", "BrightSpark Electricians", "9876543211", "Licensed electricians for home and society wiring.", true, SubscriptionTier.Free, TrustBadgeStatus.SocietyRegular, 17.4700, 78.3300,
                new[] { ("Wiring Repair", "Fix faulty wiring and switches.", "Electrical", PricingType.Hourly, (decimal?)250) }),
            ("9000000203", "GreenLeaf Pest Control", "9876543212", "Eco-friendly pest control for apartments.", true, SubscriptionTier.Free, TrustBadgeStatus.None, 17.4045, 78.3210,
                new[] { ("General Pest Treatment", "Cockroach and ant treatment.", "Pest Control", PricingType.Fixed, (decimal?)1200) }),
            ("9000000204", "MathGenius Tutoring", "9876543213", "Home tuition for grades 6-12, Math and Science.", true, SubscriptionTier.Premium, TrustBadgeStatus.IdVerified, 17.4200, 78.3000,
                new[] { ("Math Tuition", "One-on-one math coaching.", "Tutors", PricingType.Hourly, (decimal?)500) }),
            ("9000000205", "QuickFix Appliance Repair", "9876543214", "Awaiting approval — new to NestHub.", false, SubscriptionTier.Free, TrustBadgeStatus.None, 17.3000, 78.5500,
                Array.Empty<(string, string, string, PricingType, decimal?)>()),
        };

        var vendors = new List<(User User, Vendor Vendor)>();
        foreach (var (phone, business, whatsApp, bio, approved, tier, badge, latitude, longitude, services) in vendorSeeds)
        {
            var user = User.Register(PhoneNumber.Create(phone), null, passwordHash, UserType.Vendor);
            user.MarkVerified();

            var location = latitude.HasValue && longitude.HasValue ? GeoLocation.Create(latitude.Value, longitude.Value) : null;
            var vendor = Vendor.Register(user.Id, business, PhoneNumber.Create(whatsApp), bio, OperatingHours.AlwaysOpen(), location);
            if (approved)
            {
                vendor.Approve();
                if (tier == SubscriptionTier.Premium) vendor.UpgradeToPremium();
                if (badge != TrustBadgeStatus.None) vendor.AwardTrustBadge(badge);

                foreach (var (title, desc, category, pType, amount) in services)
                {
                    var pricing = pType switch
                    {
                        PricingType.Fixed => PricingInfo.Fixed(amount!.Value),
                        PricingType.Hourly => PricingInfo.Hourly(amount!.Value),
                        PricingType.StartingFrom => PricingInfo.StartingFrom(amount!.Value),
                        _ => PricingInfo.ContactForQuote()
                    };
                    vendor.AddService(title, desc, pricing, category);
                }
            }

            context.Users.Add(user);
            context.Vendors.Add(vendor);
            vendors.Add((user, vendor));
        }

        var plumber = vendors[0].Vendor;
        var electrician = vendors[1].Vendor;
        var pestControl = vendors[2].Vendor;
        var tutor = vendors[3].Vendor;

        // ---- Vendor/society affiliations ----
        // Seeded directly here (rather than relying on VendorLocationChangedDomainEvent, which
        // this seeder's single bulk SaveChangesAsync at the end never dispatches) so the demo
        // data shows every tier immediately: InHouse set explicitly, Nearby computed by hand
        // using the exact same 5km radius RecomputeVendorProximityCommand uses at runtime.
        context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(plumber.Id, lakeview.Id, AffiliationType.InHouse));
        context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(pestControl.Id, myHome.Id, AffiliationType.InHouse));
        context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(electrician.Id, lakeview.Id, AffiliationType.Nearby));
        context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(electrician.Id, greenMeadows.Id, AffiliationType.Nearby));
        context.VendorSocietyCoverages.Add(VendorSocietyCoverage.Create(tutor.Id, myHome.Id, AffiliationType.Nearby));

        // ---- Reviews (demonstrates neighbor-verified filtering: same vendor, different societies) ----
        var reviewSeeds = new (Domain.Residents.Resident Resident, Vendor Vendor, int Rating, string Comment, bool Flag)[]
        {
            (residents[0].Resident, plumber, 5, "Excellent and prompt service, highly recommend!", false),
            (residents[1].Resident, plumber, 4, "Good work, arrived a bit late but fixed the issue.", false),
            (residents[2].Resident, plumber, 2, "Overcharged compared to the quote given.", false),
            (residents[0].Resident, electrician, 5, "Very professional and quick.", false),
            (residents[1].Resident, tutor, 5, "My daughter's grades improved a lot, wonderful tutor.", false),
            (residents[3].Resident, plumber, 1, "Rude behavior, would not recommend at all.", true),
        };

        foreach (var (resident, vendor, rating, comment, flag) in reviewSeeds)
        {
            var review = Review.Submit(resident.Id, vendor.Id, resident.SocietyId, Rating.Create(rating), comment);
            review.ApplySentimentScore(aiService.ScoreSentiment(comment));
            if (flag) review.Flag();
            context.Reviews.Add(review);
        }

        foreach (var vendor in new[] { plumber, electrician, tutor })
        {
            var vendorReviews = reviewSeeds.Where(r => r.Vendor == vendor).Select(r => (double)r.Rating).ToList();
            if (vendorReviews.Count > 0)
                vendor.RecalculateAverageRating(Math.Round(vendorReviews.Average(), 2));
        }

        // ---- SOS Requests ----
        var openSos = SosRequest.RaiseNew(residents[2].Resident.Id, residents[2].Resident.SocietyId, "Plumbing", "Leaking pipe in kitchen, urgent!");
        var claimedSos = SosRequest.RaiseNew(residents[0].Resident.Id, residents[0].Resident.SocietyId, "Electrical", "Power socket sparking in living room!");
        claimedSos.ClaimBy(electrician.Id);
        context.SosRequests.AddRange(openSos, claimedSos);

        // ---- Analytics logs (last 14 days, per approved vendor) ----
        var random = new Random(42);
        foreach (var (_, vendor) in vendors.Where(v => v.Vendor.IsApproved))
        {
            for (var day = 0; day < 14; day++)
            {
                var views = random.Next(1, 6);
                for (var i = 0; i < views; i++) context.AnalyticsLogs.Add(AnalyticsLog.Record(vendor.Id, AnalyticsActionType.ProfileView));

                var calls = random.Next(0, 3);
                for (var i = 0; i < calls; i++) context.AnalyticsLogs.Add(AnalyticsLog.Record(vendor.Id, AnalyticsActionType.CallClick));

                var whatsApps = random.Next(0, 3);
                for (var i = 0; i < whatsApps; i++) context.AnalyticsLogs.Add(AnalyticsLog.Record(vendor.Id, AnalyticsActionType.WhatsAppClick));
            }
        }

        await context.SaveChangesAsync();
    }
}
