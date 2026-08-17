using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thirdpartynetwork.Core.Migrations
{
    /// <inheritdoc />
    public partial class ContentViewingLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentViewingLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentKind = table.Column<int>(type: "integer", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedForUserProxyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedForNetworkId = table.Column<Guid>(type: "uuid", nullable: true),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentViewingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentViewingLinks_Networks_IssuedForNetworkId",
                        column: x => x.IssuedForNetworkId,
                        principalTable: "Networks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentViewingLinks_UserProxies_IssuedForUserProxyId",
                        column: x => x.IssuedForUserProxyId,
                        principalTable: "UserProxies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentViewingLinks_ContentKind_ContentId",
                table: "ContentViewingLinks",
                columns: new[] { "ContentKind", "ContentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentViewingLinks_ExpiresAt",
                table: "ContentViewingLinks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContentViewingLinks_IssuedForNetworkId",
                table: "ContentViewingLinks",
                column: "IssuedForNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentViewingLinks_IssuedForUserProxyId",
                table: "ContentViewingLinks",
                column: "IssuedForUserProxyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentViewingLinks");
        }
    }
}
