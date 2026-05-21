using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedLinkFieldToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "link_address",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 21, 15, 24, 15, 534, DateTimeKind.Utc).AddTicks(8767), new DateTime(2026, 5, 21, 15, 24, 15, 534, DateTimeKind.Utc).AddTicks(8772), "6842ADAF81FB10EFD0D7F9683756B4CA804753729D4FE002C8CA9D790E4D8D81-62108E393AB02E04E7D63733AE939F5F" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "link_address",
                table: "notifications");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 18, 17, 5, 53, 119, DateTimeKind.Utc).AddTicks(7477), new DateTime(2026, 5, 18, 17, 5, 53, 119, DateTimeKind.Utc).AddTicks(7482), "2BFC2C2F46EE3A296C6A3B9A08B0B73F28FC40797D0F5C686A0020D40820B533-5D625EEDDAFF99831996E8DCB6F0AF14" });
        }
    }
}
