using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppInstallations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppInstallations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GrantedPermissions = table.Column<List<string>>(type: "text[]", nullable: false),
                    InstalledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInstallations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppInstallations_HouseholdId_AppId",
                table: "AppInstallations",
                columns: new[] { "HouseholdId", "AppId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppInstallations");
        }
    }
}
