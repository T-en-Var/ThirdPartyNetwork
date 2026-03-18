using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    public partial class addreferencestoauthor : Migration
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages");

            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages");

            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
