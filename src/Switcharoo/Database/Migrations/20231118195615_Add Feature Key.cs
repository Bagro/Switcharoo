using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Switcharoo.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Features",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Features");
        }
    }
}
