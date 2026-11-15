using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class Entitlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<JsonDocument>(
                name: "Entitlements",
                table: "Roles",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Entitlements",
                table: "NetworkUsers",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.CreateTable(
                name: "NetworkEntitlements",
                columns: table => new
                {
                    NetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowFiles = table.Column<bool>(type: "boolean", nullable: false),
                    FileCountLimit = table.Column<int>(type: "integer", nullable: false),
                    FileSizeLimit = table.Column<int>(type: "integer", nullable: false),
                    FileStorageLimit = table.Column<int>(type: "integer", nullable: false),
                    AllowBlogs = table.Column<bool>(type: "boolean", nullable: false),
                    BlogCountLimit = table.Column<int>(type: "integer", nullable: false),
                    AllowConfigurations = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigurationCountLimit = table.Column<int>(type: "integer", nullable: false),
                    AllowCustomPages = table.Column<bool>(type: "boolean", nullable: false),
                    CustomPageCountLimit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkEntitlements", x => x.NetworkId);
                    table.ForeignKey(
                        name: "FK_NetworkEntitlements_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            // --- SEED DATA FOR EXISTING NETWORKS ---
            migrationBuilder.Sql(@"
                INSERT INTO ""NetworkEntitlements"" (
                    ""NetworkId"", 
                    ""AllowFiles"", ""FileCountLimit"", ""FileSizeLimit"", ""FileStorageLimit"",
                    ""AllowBlogs"", ""BlogCountLimit"",
                    ""AllowConfigurations"", ""ConfigurationCountLimit"",
                    ""AllowCustomPages"", ""CustomPageCountLimit""
                )
                SELECT 
                    ""Id"", 
                    FALSE, 0, 0, 0, 
                    FALSE, 0, 
                    FALSE, 0, 
                    FALSE, 0
                FROM ""Networks""
                ON CONFLICT (""NetworkId"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NetworkEntitlements");

            migrationBuilder.DropColumn(
                name: "Entitlements",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Entitlements",
                table: "NetworkUsers");
        }
    }
}
