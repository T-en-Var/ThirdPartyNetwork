using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trojoaccounthub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNetworkUsersName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NetworksUsers_Networks_NetworkId",
                table: "NetworksUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworksUsers_UserProxies_UserProxyId",
                table: "NetworksUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkUserPermissions_NetworksUsers_NetworkUserId",
                table: "NetworkUserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkUserRoles_NetworksUsers_NetworkUserId",
                table: "NetworkUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NetworksUsers",
                table: "NetworksUsers");

            migrationBuilder.RenameTable(
                name: "NetworksUsers",
                newName: "NetworkUsers");

            migrationBuilder.RenameIndex(
                name: "IX_NetworksUsers_UserProxyId",
                table: "NetworkUsers",
                newName: "IX_NetworkUsers_UserProxyId");

            migrationBuilder.RenameIndex(
                name: "IX_NetworksUsers_NetworkId",
                table: "NetworkUsers",
                newName: "IX_NetworkUsers_NetworkId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NetworkUsers",
                table: "NetworkUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkUserPermissions_NetworkUsers_NetworkUserId",
                table: "NetworkUserPermissions",
                column: "NetworkUserId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkUserRoles_NetworkUsers_NetworkUserId",
                table: "NetworkUserRoles",
                column: "NetworkUserId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkUsers_Networks_NetworkId",
                table: "NetworkUsers",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkUsers_UserProxies_UserProxyId",
                table: "NetworkUsers",
                column: "UserProxyId",
                principalTable: "UserProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NetworkUserPermissions_NetworkUsers_NetworkUserId",
                table: "NetworkUserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkUserRoles_NetworkUsers_NetworkUserId",
                table: "NetworkUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkUsers_Networks_NetworkId",
                table: "NetworkUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkUsers_UserProxies_UserProxyId",
                table: "NetworkUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NetworkUsers",
                table: "NetworkUsers");

            migrationBuilder.RenameTable(
                name: "NetworkUsers",
                newName: "NetworksUsers");

            migrationBuilder.RenameIndex(
                name: "IX_NetworkUsers_UserProxyId",
                table: "NetworksUsers",
                newName: "IX_NetworksUsers_UserProxyId");

            migrationBuilder.RenameIndex(
                name: "IX_NetworkUsers_NetworkId",
                table: "NetworksUsers",
                newName: "IX_NetworksUsers_NetworkId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NetworksUsers",
                table: "NetworksUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NetworksUsers_Networks_NetworkId",
                table: "NetworksUsers",
                column: "NetworkId",
                principalTable: "Networks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworksUsers_UserProxies_UserProxyId",
                table: "NetworksUsers",
                column: "UserProxyId",
                principalTable: "UserProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkUserPermissions_NetworksUsers_NetworkUserId",
                table: "NetworkUserPermissions",
                column: "NetworkUserId",
                principalTable: "NetworksUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkUserRoles_NetworksUsers_NetworkUserId",
                table: "NetworkUserRoles",
                column: "NetworkUserId",
                principalTable: "NetworksUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
