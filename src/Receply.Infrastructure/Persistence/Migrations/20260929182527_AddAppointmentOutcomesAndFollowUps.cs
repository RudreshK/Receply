using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Receply.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentOutcomesAndFollowUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OutcomeNotificationSentUtc",
                table: "Appointment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppointmentFollowUp",
                columns: table => new
                {
                    AppointmentFollowUpId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SentByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScheduledForUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_AppointmentFollowUp", x => x.AppointmentFollowUpId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentFollowUp_AppointmentId",
                table: "AppointmentFollowUp",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentFollowUp_SentUtc_ScheduledForUtc",
                table: "AppointmentFollowUp",
                columns: new[] { "SentUtc", "ScheduledForUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentFollowUp");

            migrationBuilder.DropColumn(
                name: "OutcomeNotificationSentUtc",
                table: "Appointment");
        }
    }
}
