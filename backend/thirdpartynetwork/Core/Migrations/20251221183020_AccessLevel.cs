using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class AccessLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Networks_NetworkId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks");

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                @"UPDATE ""Files"" SET ""AccessLevel"" = 1 WHERE ""IsPublic"" = TRUE;"
            );
            
            migrationBuilder.Sql(
                @"UPDATE ""Files"" SET ""AccessLevel"" = 0 WHERE ""IsPublic"" = FALSE;"
            );
            
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Files");

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "PageBlocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "NetworkId",
                table: "PageBlocks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "NetworkUserId",
                table: "Files",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "CustomPages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PageBlocks_NetworkId",
                table: "PageBlocks",
                column: "NetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_NetworkUserId",
                table: "Files",
                column: "NetworkUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_NetworkUsers_NetworkUserId",
                table: "Files",
                column: "NetworkUserId",
                principalTable: "NetworkUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Networks_NetworkId",
                table: "Files",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_Networks_NetworkId",
                table: "PageBlocks",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_NetworkUsers_NetworkUserId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Networks_NetworkId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks");

            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_Networks_NetworkId",
                table: "PageBlocks");

            migrationBuilder.DropIndex(
                name: "IX_PageBlocks_NetworkId",
                table: "PageBlocks");

            migrationBuilder.DropIndex(
                name: "IX_Files_NetworkUserId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "PageBlocks");

            migrationBuilder.DropColumn(
                name: "NetworkId",
                table: "PageBlocks");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Files",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                @"UPDATE ""Files"" SET ""IsPublic"" = TRUE WHERE ""AccessLevel"" > 0;"
            );
            
            migrationBuilder.Sql(
                @"UPDATE ""Files"" SET ""IsPublic"" = FALSE WHERE ""AccessLevel"" = 0;"
            );

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "NetworkUserId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "CustomPages");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Networks_NetworkId",
                table: "Files",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
