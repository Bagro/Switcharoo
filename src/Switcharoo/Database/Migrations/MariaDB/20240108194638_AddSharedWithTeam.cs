using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Switcharoo.Database.Migrations.MariaDB
{
    /// <inheritdoc />
    public partial class AddSharedWithTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShareWithTeam",
                table: "Features",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShareWithTeam",
                table: "Environments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DefaultTeamAllowToggle",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DefaultTeamReadOnly",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareWithTeam",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "ShareWithTeam",
                table: "Environments");

            migrationBuilder.DropColumn(
                name: "DefaultTeamAllowToggle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultTeamReadOnly",
                table: "AspNetUsers");
        }
    }
}
