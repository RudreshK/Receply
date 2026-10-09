using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Receply.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelAccountCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "ChannelAccount",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WabaId",
                table: "ChannelAccount",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "ChannelAccount");

            migrationBuilder.DropColumn(
                name: "WabaId",
                table: "ChannelAccount");
        }
    }
}
