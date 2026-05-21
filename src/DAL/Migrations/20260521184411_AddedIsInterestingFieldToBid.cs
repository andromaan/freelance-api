using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsInterestingFieldToBid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_interesting",
                table: "bids",
                type: "boolean",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 21, 18, 44, 11, 304, DateTimeKind.Utc).AddTicks(8775), new DateTime(2026, 5, 21, 18, 44, 11, 304, DateTimeKind.Utc).AddTicks(8782), "A4E784A1E85D277506AABE4891C98A17AC7278EFEDF88DA986EA5F11B542DD81-B2181303123DF7981EC461660DDB252D" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_interesting",
                table: "bids");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 21, 15, 24, 15, 534, DateTimeKind.Utc).AddTicks(8767), new DateTime(2026, 5, 21, 15, 24, 15, 534, DateTimeKind.Utc).AddTicks(8772), "6842ADAF81FB10EFD0D7F9683756B4CA804753729D4FE002C8CA9D790E4D8D81-62108E393AB02E04E7D63733AE939F5F" });
        }
    }
}
