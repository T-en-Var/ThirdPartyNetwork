using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class RenameEntitlementsToFeatureFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renamed (not dropped/recreated) to preserve existing data.
            migrationBuilder.RenameTable(
                name: "NetworkEntitlements",
                newName: "NetworkFeatureFlags");

            migrationBuilder.Sql(
                "ALTER TABLE \"NetworkFeatureFlags\" RENAME CONSTRAINT \"PK_NetworkEntitlements\" TO \"PK_NetworkFeatureFlags\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"NetworkFeatureFlags\" RENAME CONSTRAINT \"FK_NetworkEntitlements_Networks_NetworkId\" TO \"FK_NetworkFeatureFlags_Networks_NetworkId\";");

            migrationBuilder.RenameColumn(
                name: "Entitlements",
                table: "Roles",
                newName: "FeatureFlags");

            migrationBuilder.RenameColumn(
                name: "Entitlements",
                table: "NetworkUsers",
                newName: "FeatureFlags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeatureFlags",
                table: "Roles",
                newName: "Entitlements");

            migrationBuilder.RenameColumn(
                name: "FeatureFlags",
                table: "NetworkUsers",
                newName: "Entitlements");

            migrationBuilder.Sql(
                "ALTER TABLE \"NetworkFeatureFlags\" RENAME CONSTRAINT \"FK_NetworkFeatureFlags_Networks_NetworkId\" TO \"FK_NetworkEntitlements_Networks_NetworkId\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"NetworkFeatureFlags\" RENAME CONSTRAINT \"PK_NetworkFeatureFlags\" TO \"PK_NetworkEntitlements\";");

            migrationBuilder.RenameTable(
                name: "NetworkFeatureFlags",
                newName: "NetworkEntitlements");
        }
    }
}
