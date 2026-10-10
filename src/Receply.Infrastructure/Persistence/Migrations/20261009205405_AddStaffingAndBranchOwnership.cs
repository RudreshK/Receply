using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Receply.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffingAndBranchOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Staff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "Staff",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "Staff",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "Resource",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StaffLeave",
                columns: table => new
                {
                    StaffLeaveId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffLeave", x => x.StaffLeaveId);
                    table.ForeignKey(
                        name: "FK_StaffLeave_Staff",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffService",
                columns: table => new
                {
                    StaffServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffService", x => x.StaffServiceId);
                    table.ForeignKey(
                        name: "FK_StaffService_Service",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffService_Staff",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_BranchId",
                table: "Staff",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_StaffId",
                table: "Resource",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "UQ_StaffLeave_Staff_Date",
                table: "StaffLeave",
                columns: new[] { "StaffId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffService_ServiceId",
                table: "StaffService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "UQ_StaffService_Staff_Service",
                table: "StaffService",
                columns: new[] { "StaffId", "ServiceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Resource_Staff",
                table: "Resource",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Branch",
                table: "Staff",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resource_Staff",
                table: "Resource");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Branch",
                table: "Staff");

            migrationBuilder.DropTable(
                name: "StaffLeave");

            migrationBuilder.DropTable(
                name: "StaffService");

            migrationBuilder.DropIndex(
                name: "IX_Staff_BranchId",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Resource_StaffId",
                table: "Resource");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Resource");
        }
    }
}
