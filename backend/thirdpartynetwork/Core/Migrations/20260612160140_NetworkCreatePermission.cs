using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class NetworkCreatePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_NetworkUsers_NetworkUserId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Networks_NetworkId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_NetworkUserId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "NetworkUserId",
                table: "Files");

            migrationBuilder.AddColumn<Guid>(
                name: "NetworkId",
                table: "Permissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_NetworkId",
                table: "Permissions",
                column: "NetworkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Networks_NetworkId",
                table: "Files",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Networks_NetworkId",
                table: "Permissions",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Networks_NetworkId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Networks_NetworkId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_NetworkId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "NetworkId",
                table: "Permissions");

            migrationBuilder.AddColumn<Guid>(
                name: "NetworkUserId",
                table: "Files",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_NetworkUserId",
                table: "Files",
                column: "NetworkUserId");

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
        }
    }
}
