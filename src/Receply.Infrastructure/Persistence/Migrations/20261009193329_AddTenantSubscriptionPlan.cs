using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Receply.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Plan",
                table: "Tenant",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PlanRenewsUtc",
                table: "Tenant",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TrialEndsUtc",
                table: "Tenant",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "PlanRenewsUtc",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "TrialEndsUtc",
                table: "Tenant");
        }
    }
}
