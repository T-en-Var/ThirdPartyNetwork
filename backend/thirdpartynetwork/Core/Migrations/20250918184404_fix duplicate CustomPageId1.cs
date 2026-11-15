using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class fixduplicateCustomPageId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_CustomPages_CustomPageId1",
                table: "PageBlocks");

            migrationBuilder.DropIndex(
                name: "IX_PageBlocks_CustomPageId1",
                table: "PageBlocks");

            migrationBuilder.DropColumn(
                name: "CustomPageId1",
                table: "PageBlocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomPageId1",
                table: "PageBlocks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PageBlocks_CustomPageId1",
                table: "PageBlocks",
                column: "CustomPageId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_CustomPages_CustomPageId1",
                table: "PageBlocks",
                column: "CustomPageId1",
                principalTable: "CustomPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
