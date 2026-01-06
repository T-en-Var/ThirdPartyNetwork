using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class CoverImageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs",
                column: "CoverImageId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs",
                column: "CoverImageId",
                principalTable: "Files",
                principalColumn: "Id");
        }
    }
}
