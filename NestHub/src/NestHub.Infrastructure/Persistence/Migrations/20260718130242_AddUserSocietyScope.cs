using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NestHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSocietyScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SocietyId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SocietyId",
                table: "Users",
                column: "SocietyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Societies_SocietyId",
                table: "Users",
                column: "SocietyId",
                principalTable: "Societies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Societies_SocietyId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SocietyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SocietyId",
                table: "Users");
        }
    }
}
