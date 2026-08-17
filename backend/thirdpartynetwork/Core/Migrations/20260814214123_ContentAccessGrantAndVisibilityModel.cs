using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class ContentAccessGrantAndVisibilityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_NetworkUsers_AuthorId",
                table: "Blogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Configurations_NetworkUsers_AuthorId",
                table: "Configurations");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_NetworkUsers_AuthorId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks");

            migrationBuilder.DropIndex(
                name: "IX_PageBlocks_AuthorId",
                table: "PageBlocks");

            migrationBuilder.DropIndex(
                name: "IX_Files_AuthorId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_CustomPages_AuthorId",
                table: "CustomPages");

            migrationBuilder.DropIndex(
                name: "IX_Configurations_AuthorId",
                table: "Configurations");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_AuthorId",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "PageBlocks");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "CustomPages");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Blogs");

            // NOT a rename: AccessLevel (Private=0/PublicToNetwork=1/Public=2) and
            // OwnershipMode (AuthorOwned=0/NetworkOwned=1) are unrelated enums that happen
            // to share a column "slot" in EF's diff — scaffolding this as RenameColumn would
            // silently reinterpret every existing AccessLevel=2 (Public) row as an undefined
            // OwnershipMode value. OwnershipMode is a genuinely NEW concept with no
            // predecessor to derive from, so every existing row gets the model's own default
            // (AuthorOwned=0) via a fresh column; the old AccessLevel column is read one more
            // time for the ContentAccessGrant backfill below, then dropped for real.
            migrationBuilder.AddColumn<int>(
                name: "OwnershipMode",
                table: "PageBlocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OwnershipMode",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OwnershipMode",
                table: "CustomPages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OwnershipMode",
                table: "Configurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OwnershipMode",
                table: "Blogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserProxyId",
                table: "PageBlocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PageBlocks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "PageBlocks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalContentKinds",
                table: "Networks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserProxyId",
                table: "Files",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserProxyId",
                table: "CustomPages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CustomPages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "CustomPages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserProxyId",
                table: "Configurations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Configurations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Configurations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserProxyId",
                table: "Blogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Blogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContentAccessGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentKind = table.Column<int>(type: "integer", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GranteeType = table.Column<int>(type: "integer", nullable: false),
                    GranteeNetworkId = table.Column<Guid>(type: "uuid", nullable: true),
                    GranteeRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    GranteePermissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    GranteeUserProxyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Capabilities = table.Column<int>(type: "integer", nullable: false),
                    GrantedByNetworkUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentAccessGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentAccessGrants_NetworkUsers_GrantedByNetworkUserId",
                        column: x => x.GrantedByNetworkUserId,
                        principalTable: "NetworkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentAccessGrants_Networks_GranteeNetworkId",
                        column: x => x.GranteeNetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentAccessGrants_Permissions_GranteePermissionId",
                        column: x => x.GranteePermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentAccessGrants_Roles_GranteeRoleId",
                        column: x => x.GranteeRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentAccessGrants_UserProxies_GranteeUserProxyId",
                        column: x => x.GranteeUserProxyId,
                        principalTable: "UserProxies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageBlocks_AuthorUserProxyId",
                table: "PageBlocks",
                column: "AuthorUserProxyId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_AuthorUserProxyId",
                table: "Files",
                column: "AuthorUserProxyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomPages_AuthorUserProxyId",
                table: "CustomPages",
                column: "AuthorUserProxyId");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_AuthorUserProxyId",
                table: "Configurations",
                column: "AuthorUserProxyId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AuthorUserProxyId",
                table: "Blogs",
                column: "AuthorUserProxyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAccessGrants_ContentKind_ContentId",
                table: "ContentAccessGrants",
                columns: new[] { "ContentKind", "ContentId" },
                filter: "\"RevokedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAccessGrants_GrantedByNetworkUserId",
                table: "ContentAccessGrants",
                column: "GrantedByNetworkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAccessGrants_GranteeNetworkId_ContentKind",
                table: "ContentAccessGrants",
                columns: new[] { "GranteeNetworkId", "ContentKind" },
                filter: "\"RevokedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAccessGrants_GranteePermissionId",
                table: "ContentAccessGrants",
                column: "GranteePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAccessGrants_GranteeRoleId",
                table: "ContentAccessGrants",
                column: "GranteeRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAccessGrants_GranteeUserProxyId_ContentKind",
                table: "ContentAccessGrants",
                columns: new[] { "GranteeUserProxyId", "ContentKind" },
                filter: "\"RevokedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_UserProxies_AuthorUserProxyId",
                table: "Blogs",
                column: "AuthorUserProxyId",
                principalTable: "UserProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Configurations_UserProxies_AuthorUserProxyId",
                table: "Configurations",
                column: "AuthorUserProxyId",
                principalTable: "UserProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPages_UserProxies_AuthorUserProxyId",
                table: "CustomPages",
                column: "AuthorUserProxyId",
                principalTable: "UserProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_UserProxies_AuthorUserProxyId",
                table: "Files",
                column: "AuthorUserProxyId",
                principalTable: "UserProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_UserProxies_AuthorUserProxyId",
                table: "PageBlocks",
                column: "AuthorUserProxyId",
                principalTable: "UserProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // --- PLAN.md §7 step 3.2: backfill, in this order, before AccessLevel is dropped ---

            // Nothing was "draft" under the old schema, so nothing should retroactively
            // become Draft — every existing row is published as of right now.
            migrationBuilder.Sql("UPDATE \"Files\" SET \"PublishedAt\" = \"CreatedOn\";");
            migrationBuilder.Sql("UPDATE \"Blogs\" SET \"PublishedAt\" = \"CreatedOn\";");
            migrationBuilder.Sql("UPDATE \"Configurations\" SET \"PublishedAt\" = \"CreatedOn\";");
            migrationBuilder.Sql("UPDATE \"CustomPages\" SET \"PublishedAt\" = \"CreatedOn\";");
            migrationBuilder.Sql("UPDATE \"PageBlocks\" SET \"PublishedAt\" = \"CreatedOn\";");

            // Old AccessLevel.PublicToNetwork (1) rows: this is exactly "my own network can
            // already see this," which is no longer free (§2.1) — insert the equivalent
            // ContentAccessGrant{GranteeType=Network, GranteeNetworkId=own network} so
            // existing sharing keeps working post-migration. ContentKind ints match
            // Core/Enum/ContentKind.cs (File=0, Blog=1, Configuration=2, CustomPage=3,
            // PageBlock=4); GranteeType=0 is Network; Capabilities=1 is View.
            //
            // Old AccessLevel.Public (2) rows get the SAME grant, on the mechanical
            // majority-case assumption that the row's own network covers it — per §7 step
            // 3.2 this is explicitly a case-by-case call for content whose network is NOT
            // itself Network.IsPublic (§0.3's anonymous-reuse path only fires for IsPublic
            // networks): such rows lose true anonymous/public-internet reachability until
            // either the network sets IsPublic=true or someone adds a broader grant by hand.
            // A migration can get the mechanical majority case right; it can't make that
            // per-network product decision for you.
            // gen_random_uuid() needs PostgreSQL 13+ (built into core, no extension needed).
            migrationBuilder.Sql("""
                INSERT INTO "ContentAccessGrants" ("Id", "ContentKind", "ContentId", "GranteeType", "GranteeNetworkId", "Capabilities", "CreatedOn", "UpdatedAt")
                SELECT gen_random_uuid(), 0, "Id", 0, "NetworkId", 1, now(), now() FROM "Files" WHERE "AccessLevel" > 0;
                """);
            migrationBuilder.Sql("""
                INSERT INTO "ContentAccessGrants" ("Id", "ContentKind", "ContentId", "GranteeType", "GranteeNetworkId", "Capabilities", "CreatedOn", "UpdatedAt")
                SELECT gen_random_uuid(), 1, "Id", 0, "NetworkId", 1, now(), now() FROM "Blogs" WHERE "AccessLevel" > 0;
                """);
            migrationBuilder.Sql("""
                INSERT INTO "ContentAccessGrants" ("Id", "ContentKind", "ContentId", "GranteeType", "GranteeNetworkId", "Capabilities", "CreatedOn", "UpdatedAt")
                SELECT gen_random_uuid(), 2, "Id", 0, "NetworkId", 1, now(), now() FROM "Configurations" WHERE "AccessLevel" > 0;
                """);
            migrationBuilder.Sql("""
                INSERT INTO "ContentAccessGrants" ("Id", "ContentKind", "ContentId", "GranteeType", "GranteeNetworkId", "Capabilities", "CreatedOn", "UpdatedAt")
                SELECT gen_random_uuid(), 3, "Id", 0, "NetworkId", 1, now(), now() FROM "CustomPages" WHERE "AccessLevel" > 0;
                """);
            migrationBuilder.Sql("""
                INSERT INTO "ContentAccessGrants" ("Id", "ContentKind", "ContentId", "GranteeType", "GranteeNetworkId", "Capabilities", "CreatedOn", "UpdatedAt")
                SELECT gen_random_uuid(), 4, "Id", 0, "NetworkId", 1, now(), now() FROM "PageBlocks" WHERE "AccessLevel" > 0;
                """);

            // Only now is the old column safe to drop — every row that needs a
            // ContentAccessGrant to keep working has one.
            migrationBuilder.DropColumn(name: "AccessLevel", table: "Files");
            migrationBuilder.DropColumn(name: "AccessLevel", table: "Blogs");
            migrationBuilder.DropColumn(name: "AccessLevel", table: "Configurations");
            migrationBuilder.DropColumn(name: "AccessLevel", table: "CustomPages");
            migrationBuilder.DropColumn(name: "AccessLevel", table: "PageBlocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_UserProxies_AuthorUserProxyId",
                table: "Blogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Configurations_UserProxies_AuthorUserProxyId",
                table: "Configurations");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomPages_UserProxies_AuthorUserProxyId",
                table: "CustomPages");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_UserProxies_AuthorUserProxyId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_PageBlocks_UserProxies_AuthorUserProxyId",
                table: "PageBlocks");

            migrationBuilder.DropTable(
                name: "ContentAccessGrants");

            migrationBuilder.DropIndex(
                name: "IX_PageBlocks_AuthorUserProxyId",
                table: "PageBlocks");

            migrationBuilder.DropIndex(
                name: "IX_Files_AuthorUserProxyId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_CustomPages_AuthorUserProxyId",
                table: "CustomPages");

            migrationBuilder.DropIndex(
                name: "IX_Configurations_AuthorUserProxyId",
                table: "Configurations");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_AuthorUserProxyId",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "AuthorUserProxyId",
                table: "PageBlocks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PageBlocks");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "PageBlocks");

            migrationBuilder.DropColumn(
                name: "ExternalContentKinds",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "AuthorUserProxyId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuthorUserProxyId",
                table: "CustomPages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CustomPages");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "CustomPages");

            migrationBuilder.DropColumn(
                name: "AuthorUserProxyId",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "AuthorUserProxyId",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Blogs");

            // Best-effort schema-only reversal — NOT a data-preserving rename (see Up()):
            // OwnershipMode is dropped and AccessLevel comes back as a fresh Private(0)
            // column. The original AccessLevel values (and the ContentAccessGrant rows the
            // Up() backfill derived from them) are not reconstructed; Down() migrations are
            // schema rollback, not a data time machine.
            migrationBuilder.DropColumn(name: "OwnershipMode", table: "PageBlocks");
            migrationBuilder.DropColumn(name: "OwnershipMode", table: "Files");
            migrationBuilder.DropColumn(name: "OwnershipMode", table: "CustomPages");
            migrationBuilder.DropColumn(name: "OwnershipMode", table: "Configurations");
            migrationBuilder.DropColumn(name: "OwnershipMode", table: "Blogs");

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "PageBlocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "CustomPages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "Configurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "Blogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "PageBlocks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Files",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "CustomPages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Configurations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Blogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PageBlocks_AuthorId",
                table: "PageBlocks",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_AuthorId",
                table: "Files",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomPages_AuthorId",
                table: "CustomPages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_AuthorId",
                table: "Configurations",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AuthorId",
                table: "Blogs",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_NetworkUsers_AuthorId",
                table: "Blogs",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Configurations_NetworkUsers_AuthorId",
                table: "Configurations",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomPages_NetworkUsers_AuthorId",
                table: "CustomPages",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_NetworkUsers_AuthorId",
                table: "Files",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageBlocks_NetworkUsers_AuthorId",
                table: "PageBlocks",
                column: "AuthorId",
                principalTable: "NetworkUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
