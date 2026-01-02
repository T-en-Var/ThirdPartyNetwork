using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class nullablecoverimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoverImageId",
                table: "Blogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs",
                column: "CoverImageId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoverImageId",
                table: "Blogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_Files_CoverImageId",
                table: "Blogs",
                column: "CoverImageId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
