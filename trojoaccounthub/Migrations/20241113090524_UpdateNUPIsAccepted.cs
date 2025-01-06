using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trojoaccounthub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNUPIsAccepted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsRequired",
                table: "NetworkUserPermissions",
                newName: "IsAccepted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAccepted",
                table: "NetworkUserPermissions",
                newName: "IsRequired");
        }
    }
}
