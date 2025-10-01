using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class FormatNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Networks_Files_ImageFileId",
                table: "Networks");

            migrationBuilder.AlterColumn<string>(
                name: "Format",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Networks_Files_ImageFileId",
                table: "Networks",
                column: "ImageFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Networks_Files_ImageFileId",
                table: "Networks");

            migrationBuilder.AlterColumn<string>(
                name: "Format",
                table: "Files",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Networks_Files_ImageFileId",
                table: "Networks",
                column: "ImageFileId",
                principalTable: "Files",
                principalColumn: "Id");
        }
    }
}
