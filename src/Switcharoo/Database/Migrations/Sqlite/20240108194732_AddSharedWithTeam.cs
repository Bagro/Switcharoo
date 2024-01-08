using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Switcharoo.Database.Migrations.Sqlite
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
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShareWithTeam",
                table: "Environments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DefaultTeamAllowToggle",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DefaultTeamReadOnly",
                table: "AspNetUsers",
                type: "INTEGER",
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
