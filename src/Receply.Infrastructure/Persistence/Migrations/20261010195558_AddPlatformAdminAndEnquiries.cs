using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Receply.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAdminAndEnquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessEnquiry",
                columns: table => new
                {
                    BusinessEnquiryId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BusinessType = table.Column<int>(type: "integer", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConvertedTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEnquiry", x => x.BusinessEnquiryId);
                });

            migrationBuilder.CreateTable(
                name: "PlatformAdmin",
                columns: table => new
                {
                    PlatformAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAdmin", x => x.PlatformAdminId);
                });

            migrationBuilder.CreateTable(
                name: "PlatformAdminLoginOtp",
                columns: table => new
                {
                    PlatformAdminLoginOtpId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAdminLoginOtp", x => x.PlatformAdminLoginOtpId);
                    table.ForeignKey(
                        name: "FK_PlatformAdminLoginOtp_PlatformAdmin",
                        column: x => x.PlatformAdminId,
                        principalTable: "PlatformAdmin",
                        principalColumn: "PlatformAdminId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_PlatformAdmin_PhoneNumber",
                table: "PlatformAdmin",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdminLoginOtp_PlatformAdminId",
                table: "PlatformAdminLoginOtp",
                column: "PlatformAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessEnquiry");

            migrationBuilder.DropTable(
                name: "PlatformAdminLoginOtp");

            migrationBuilder.DropTable(
                name: "PlatformAdmin");
        }
    }
}
