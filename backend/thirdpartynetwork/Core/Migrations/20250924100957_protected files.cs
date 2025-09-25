using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class protectedfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemProtected",
                table: "Files",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystemProtected",
                table: "Files");
        }
    }
}
