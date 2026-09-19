using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Receply.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Appointment", x => x.AppointmentId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelAccount",
                columns: table => new
                {
                    ChannelAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ChannelAccount", x => x.ChannelAccountId);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
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
                    table.PrimaryKey("PK_Client", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Conversation",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Conversation", x => x.ConversationId);
                });

            migrationBuilder.CreateTable(
                name: "Resource",
                columns: table => new
                {
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Resource", x => x.ResourceId);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Service", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Staff", x => x.StaffId);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BusinessType = table.Column<int>(type: "integer", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "TimeBlock",
                columns: table => new
                {
                    TimeBlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TimeBlock", x => x.TimeBlockId);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Sender = table.Column<int>(type: "integer", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Message_Conversation",
                        column: x => x.ConversationId,
                        principalTable: "Conversation",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityRule",
                columns: table => new
                {
                    AvailabilityRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
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
                    table.PrimaryKey("PK_AvailabilityRule", x => x.AvailabilityRuleId);
                    table.ForeignKey(
                        name: "FK_AvailabilityRule_Resource",
                        column: x => x.ResourceId,
                        principalTable: "Resource",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Branch", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branch_Tenant",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ResourceId_StartUtc_EndUtc",
                table: "Appointment",
                columns: new[] { "ResourceId", "StartUtc", "EndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_TenantId_ClientId",
                table: "Appointment",
                columns: new[] { "TenantId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityRule_ResourceId_DayOfWeek",
                table: "AvailabilityRule",
                columns: new[] { "ResourceId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_Branch_TenantId",
                table: "Branch",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ_ChannelAccount_Type_ExternalId",
                table: "ChannelAccount",
                columns: new[] { "Type", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Client_Tenant_PhoneNumber",
                table: "Client",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_ClientId",
                table: "Conversation",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_TenantId_Status",
                table: "Conversation",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Message_ConversationId_SentAtUtc",
                table: "Message",
                columns: new[] { "ConversationId", "SentAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Resource_TenantId_BranchId",
                table: "Resource",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Service_TenantId",
                table: "Service",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ_Staff_Tenant_Email",
                table: "Staff",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeBlock_ResourceId_StartUtc_EndUtc",
                table: "TimeBlock",
                columns: new[] { "ResourceId", "StartUtc", "EndUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "AvailabilityRule");

            migrationBuilder.DropTable(
                name: "Branch");

            migrationBuilder.DropTable(
                name: "ChannelAccount");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "TimeBlock");

            migrationBuilder.DropTable(
                name: "Resource");

            migrationBuilder.DropTable(
                name: "Tenant");

            migrationBuilder.DropTable(
                name: "Conversation");
        }
    }
}
