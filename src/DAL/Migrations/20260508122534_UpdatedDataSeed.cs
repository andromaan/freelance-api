using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "skills",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 8, 12, 25, 33, 804, DateTimeKind.Utc).AddTicks(8893), new DateTime(2026, 5, 8, 12, 25, 33, 804, DateTimeKind.Utc).AddTicks(8897), "68FCD63C8B7DF2858F42974B089A89817D7B684DEFF0E27F88172C54EB2D4504-A265FCB45730900F125D16EC3447496D" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "skills",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "C#" },
                    { 2, "Java" },
                    { 3, "Python" },
                    { 4, "JavaScript" },
                    { 5, "SQL" },
                    { 6, "AWS" },
                    { 7, "Azure" },
                    { 8, "Docker" },
                    { 9, "Kubernetes" },
                    { 10, "React" }
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 3, 3, 11, 47, 45, 436, DateTimeKind.Utc).AddTicks(4460), new DateTime(2026, 3, 3, 11, 47, 45, 436, DateTimeKind.Utc).AddTicks(4466), "885C7FB0AAB332EA00C5CE8B25CC9AF52CC3FE74BC1995AEE2628F04A442277D-4FFDC4EA29B9A0A2AEAB8EF9B43EFFC4" });
        }
    }
}
