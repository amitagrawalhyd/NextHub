using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace NestHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorAndSocietyLocationAndAffiliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AffiliationType",
                table: "VendorSocietyCoverages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Vendors",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Societies",
                type: "geography",
                nullable: true);

            // Data migration: backfill Societies.Location from the packed "lat,lng" nvarchar
            // GeoLocation column BEFORE it's dropped below, so existing society coordinates
            // survive the switch to a real spatial type.
            migrationBuilder.Sql(@"
                UPDATE dbo.Societies
                SET Location = geography::Point(
                    CAST(LEFT(GeoLocation, CHARINDEX(',', GeoLocation) - 1) AS FLOAT),
                    CAST(SUBSTRING(GeoLocation, CHARINDEX(',', GeoLocation) + 1, LEN(GeoLocation)) AS FLOAT),
                    4326)
                WHERE GeoLocation IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "GeoLocation",
                table: "Societies");

            migrationBuilder.CreateIndex(
                name: "UX_VendorSocietyCoverages_Vendor_InHouse",
                table: "VendorSocietyCoverages",
                column: "VendorId",
                unique: true,
                filter: "[AffiliationType] = 'InHouse'");

            migrationBuilder.Sql("CREATE SPATIAL INDEX SIX_Societies_Location ON dbo.Societies(Location) USING GEOGRAPHY_AUTO_GRID;");
            migrationBuilder.Sql("CREATE SPATIAL INDEX SIX_Vendors_Location ON dbo.Vendors(Location) USING GEOGRAPHY_AUTO_GRID;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX SIX_Societies_Location ON dbo.Societies;");
            migrationBuilder.Sql("DROP INDEX SIX_Vendors_Location ON dbo.Vendors;");

            migrationBuilder.DropIndex(
                name: "UX_VendorSocietyCoverages_Vendor_InHouse",
                table: "VendorSocietyCoverages");

            migrationBuilder.AddColumn<string>(
                name: "GeoLocation",
                table: "Societies",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE dbo.Societies
                SET GeoLocation = CAST(Location.Lat AS NVARCHAR(32)) + ',' + CAST(Location.Long AS NVARCHAR(32))
                WHERE Location IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "AffiliationType",
                table: "VendorSocietyCoverages");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Societies");
        }
    }
}
