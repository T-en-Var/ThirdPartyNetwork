using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class NetworkInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePicture",
                table: "UserProxies",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Networks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Networks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Networks");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "UserProxies",
                newName: "ProfilePicture");
        }
    }
}
