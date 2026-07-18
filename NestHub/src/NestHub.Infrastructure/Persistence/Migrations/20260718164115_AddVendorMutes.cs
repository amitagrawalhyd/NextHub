using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NestHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorMutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendorMutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorMutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorMutes_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorMutes_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorMutes_ResidentId_VendorId",
                table: "VendorMutes",
                columns: new[] { "ResidentId", "VendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorMutes_VendorId",
                table: "VendorMutes",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorMutes");
        }
    }
}
