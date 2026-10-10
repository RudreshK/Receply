using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Receply.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessEmail",
                table: "Tenant",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "Tenant",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoDataUrl",
                table: "Tenant",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Tenant",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPhotoDataUrl",
                table: "Tenant",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Tenant",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessEmail",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "LogoDataUrl",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "OwnerPhotoDataUrl",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Tenant");
        }
    }
}
