using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPropertyConstraintForWalletTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bids_project_project_id",
                table: "bids");

            migrationBuilder.DropForeignKey(
                name: "fk_contracts_project_project_id",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "fk_freelancers_skills_skill_skills_id",
                table: "freelancers_skills");

            migrationBuilder.DropForeignKey(
                name: "fk_users_country_country_id",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "transaction_type",
                table: "wallet_transactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 8, 13, 55, 33, 105, DateTimeKind.Utc).AddTicks(581), new DateTime(2026, 5, 8, 13, 55, 33, 105, DateTimeKind.Utc).AddTicks(587), "4310DE75A6FEBFC8187628227842716775DF0EAAB4140E98B4EE9BA483347784-443463310447EDA6CA103472D637D919" });

            migrationBuilder.AddForeignKey(
                name: "fk_bids_projects_project_id",
                table: "bids",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_contracts_projects_project_id",
                table: "contracts",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_freelancers_skills_skills_skills_id",
                table: "freelancers_skills",
                column: "skills_id",
                principalTable: "skills",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_users_countries_country_id",
                table: "users",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bids_projects_project_id",
                table: "bids");

            migrationBuilder.DropForeignKey(
                name: "fk_contracts_projects_project_id",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "fk_freelancers_skills_skills_skills_id",
                table: "freelancers_skills");

            migrationBuilder.DropForeignKey(
                name: "fk_users_countries_country_id",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "transaction_type",
                table: "wallet_transactions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "modified_at", "password_hash" },
                values: new object[] { new DateTime(2026, 5, 8, 12, 25, 33, 804, DateTimeKind.Utc).AddTicks(8893), new DateTime(2026, 5, 8, 12, 25, 33, 804, DateTimeKind.Utc).AddTicks(8897), "68FCD63C8B7DF2858F42974B089A89817D7B684DEFF0E27F88172C54EB2D4504-A265FCB45730900F125D16EC3447496D" });

            migrationBuilder.AddForeignKey(
                name: "fk_bids_project_project_id",
                table: "bids",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_contracts_project_project_id",
                table: "contracts",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_freelancers_skills_skill_skills_id",
                table: "freelancers_skills",
                column: "skills_id",
                principalTable: "skills",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_users_country_country_id",
                table: "users",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
