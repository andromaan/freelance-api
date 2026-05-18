using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovedStatusFromProjectMilestones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "project_milestones");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 18, 17, 5, 53, 119, DateTimeKind.Utc).AddTicks(7477), new DateTime(2026, 5, 18, 17, 5, 53, 119, DateTimeKind.Utc).AddTicks(7482), "2BFC2C2F46EE3A296C6A3B9A08B0B73F28FC40797D0F5C686A0020D40820B533-5D625EEDDAFF99831996E8DCB6F0AF14" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "project_milestones",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 8, 13, 55, 33, 105, DateTimeKind.Utc).AddTicks(581), new DateTime(2026, 5, 8, 13, 55, 33, 105, DateTimeKind.Utc).AddTicks(587), "4310DE75A6FEBFC8187628227842716775DF0EAAB4140E98B4EE9BA483347784-443463310447EDA6CA103472D637D919" });
        }
    }
}
