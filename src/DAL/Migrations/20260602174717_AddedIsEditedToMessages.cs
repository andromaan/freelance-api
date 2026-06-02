using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsEditedToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_logo",
                table: "freelancers");

            migrationBuilder.AlterColumn<bool>(
                name: "is_read",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "is_edited",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 6, 2, 17, 47, 16, 688, DateTimeKind.Utc).AddTicks(1812), new DateTime(2026, 6, 2, 17, 47, 16, 688, DateTimeKind.Utc).AddTicks(1817), "5E66818BF2E1A917DFC0F5DD98891A47C9102C207F87FA54195E63E0B593474A-82C38DBF5B9D4761EA4182C8A574E61E" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_edited",
                table: "messages");

            migrationBuilder.AlterColumn<bool>(
                name: "is_read",
                table: "messages",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "avatar_logo",
                table: "freelancers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 6, 1, 17, 56, 2, 642, DateTimeKind.Utc).AddTicks(6137), new DateTime(2026, 6, 1, 17, 56, 2, 642, DateTimeKind.Utc).AddTicks(6142), "8E1D63A6EB18F194CA848854E95A4F442A447631F863039922792ACD0EAD8D7B-CF8437025AB72D511F92AA5E780D0E61" });
        }
    }
}
