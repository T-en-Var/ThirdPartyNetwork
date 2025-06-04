using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class NetworkRedirectURI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedirectURI",
                table: "Networks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectURI",
                table: "Networks");
        }
    }
}
