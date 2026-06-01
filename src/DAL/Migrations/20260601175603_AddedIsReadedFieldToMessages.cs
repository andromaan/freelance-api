using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsReadedFieldToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_read",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 6, 1, 17, 56, 2, 642, DateTimeKind.Utc).AddTicks(6137), new DateTime(2026, 6, 1, 17, 56, 2, 642, DateTimeKind.Utc).AddTicks(6142), "8E1D63A6EB18F194CA848854E95A4F442A447631F863039922792ACD0EAD8D7B-CF8437025AB72D511F92AA5E780D0E61" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_read",
                table: "messages");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 21, 18, 44, 11, 304, DateTimeKind.Utc).AddTicks(8775), new DateTime(2026, 5, 21, 18, 44, 11, 304, DateTimeKind.Utc).AddTicks(8782), "A4E784A1E85D277506AABE4891C98A17AC7278EFEDF88DA986EA5F11B542DD81-B2181303123DF7981EC461660DDB252D" });
        }
    }
}
