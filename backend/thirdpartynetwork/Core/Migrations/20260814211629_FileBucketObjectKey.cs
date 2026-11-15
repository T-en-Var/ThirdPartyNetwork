using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class FileBucketObjectKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE all files in Coudinary are not in minio
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Files",
                newName: "ObjectKey");

            migrationBuilder.AddColumn<string>(
                name: "Bucket",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "private");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ObjectKey",
                table: "Files",
                column: "ObjectKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Files_ObjectKey",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Bucket",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "ObjectKey",
                table: "Files",
                newName: "Url");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Files",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
