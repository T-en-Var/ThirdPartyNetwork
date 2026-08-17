using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class ContentModerationFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentModerationFlags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentKind = table.Column<int>(type: "integer", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    NetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlaggedByNetworkUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HiddenInNetwork = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentModerationFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentModerationFlags_NetworkUsers_FlaggedByNetworkUserId",
                        column: x => x.FlaggedByNetworkUserId,
                        principalTable: "NetworkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentModerationFlags_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentModerationFlags_ContentKind_ContentId_NetworkId",
                table: "ContentModerationFlags",
                columns: new[] { "ContentKind", "ContentId", "NetworkId" },
                filter: "\"HiddenInNetwork\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModerationFlags_FlaggedByNetworkUserId",
                table: "ContentModerationFlags",
                column: "FlaggedByNetworkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentModerationFlags_NetworkId",
                table: "ContentModerationFlags",
                column: "NetworkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentModerationFlags");
        }
    }
}
