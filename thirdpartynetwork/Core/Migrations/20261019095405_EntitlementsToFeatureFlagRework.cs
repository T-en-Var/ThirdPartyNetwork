using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class EntitlementsToFeatureFlagRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeatureFlags",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "FeatureFlags",
                table: "NetworkUsers");

            migrationBuilder.CreateTable(
                name: "NetworkUserFeatureFlags",
                columns: table => new
                {
                    NetworkUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AllowFiles = table.Column<bool>(type: "boolean", nullable: true),
                    FileCountLimit = table.Column<int>(type: "integer", nullable: true),
                    FileSizeLimit = table.Column<int>(type: "integer", nullable: true),
                    FileStorageLimit = table.Column<int>(type: "integer", nullable: true),
                    AllowBlogs = table.Column<bool>(type: "boolean", nullable: true),
                    BlogCountLimit = table.Column<int>(type: "integer", nullable: true),
                    AllowConfigurations = table.Column<bool>(type: "boolean", nullable: true),
                    ConfigurationCountLimit = table.Column<int>(type: "integer", nullable: true),
                    AllowCustomPages = table.Column<bool>(type: "boolean", nullable: true),
                    CustomPageCountLimit = table.Column<int>(type: "integer", nullable: true),
                    CustomPageBlockCountLimit = table.Column<int>(type: "integer", nullable: true),
                    CustomPageBlockSizeLimit = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkUserFeatureFlags", x => x.NetworkUserId);
                    table.ForeignKey(
                        name: "FK_NetworkUserFeatureFlags_NetworkUsers_NetworkUserId",
                        column: x => x.NetworkUserId,
                        principalTable: "NetworkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleFeatureFlags",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AllowFiles = table.Column<bool>(type: "boolean", nullable: true),
                    FileCountLimit = table.Column<int>(type: "integer", nullable: true),
                    FileSizeLimit = table.Column<int>(type: "integer", nullable: true),
                    FileStorageLimit = table.Column<int>(type: "integer", nullable: true),
                    AllowBlogs = table.Column<bool>(type: "boolean", nullable: true),
                    BlogCountLimit = table.Column<int>(type: "integer", nullable: true),
                    AllowConfigurations = table.Column<bool>(type: "boolean", nullable: true),
                    ConfigurationCountLimit = table.Column<int>(type: "integer", nullable: true),
                    AllowCustomPages = table.Column<bool>(type: "boolean", nullable: true),
                    CustomPageCountLimit = table.Column<int>(type: "integer", nullable: true),
                    CustomPageBlockCountLimit = table.Column<int>(type: "integer", nullable: true),
                    CustomPageBlockSizeLimit = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleFeatureFlags", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_RoleFeatureFlags_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NetworkUserFeatureFlags");

            migrationBuilder.DropTable(
                name: "RoleFeatureFlags");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "FeatureFlags",
                table: "Roles",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "FeatureFlags",
                table: "NetworkUsers",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'");
        }
    }
}
