using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class CustomPageBlockEntitlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomPageBlockCountLimit",
                table: "NetworkEntitlements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomPageBlockSizeLimit",
                table: "NetworkEntitlements",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomPageBlockCountLimit",
                table: "NetworkEntitlements");

            migrationBuilder.DropColumn(
                name: "CustomPageBlockSizeLimit",
                table: "NetworkEntitlements");
        }
    }
}
