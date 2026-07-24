using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatReadState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatReadStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatReadStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatReadStates_HouseholdId_UserId",
                table: "ChatReadStates",
                columns: new[] { "HouseholdId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatReadStates");
        }
    }
}
