using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Raytha.Domain.Entities;

#nullable disable

namespace Raytha.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class v1_4_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_WebTemplates_WebTemplateId",
                table: "ContentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Views_WebTemplates_WebTemplateId",
                table: "Views");

            migrationBuilder.DropIndex(
                name: "IX_WebTemplates_DeveloperName",
                table: "WebTemplates");

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeveloperName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCanExport = table.Column<bool>(type: "bit", nullable: false),
                    PreviewImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Themes_MediaItems_PreviewImageId",
                        column: x => x.PreviewImageId,
                        principalTable: "MediaItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Themes_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Themes_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            var defaultThemeId = Guid.NewGuid();
            migrationBuilder.AddColumn<Guid>(
                name: "ThemeId",
                table: "WebTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: defaultThemeId);

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title", "DeveloperName", "IsActive", "IsCanExport", "Description", "CreationTime" },
                values: new object[,]
                {
                    { defaultThemeId, "Raytha default theme", Theme.DEFAULT_THEME_DEVELOPER_NAME, true, false, "Raytha default theme", DateTime.UtcNow },
                });

            migrationBuilder.CreateTable(
                name: "ThemeAccessToMediaItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeAccessToMediaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeAccessToMediaItems_MediaItems_MediaItemId",
                        column: x => x.MediaItemId,
                        principalTable: "MediaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThemeAccessToMediaItems_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThemeRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebTemplatesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebTemplatesMappingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeRevisions_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThemeRevisions_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThemeRevisions_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThemeWebTemplatesMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ViewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeWebTemplatesMappings", x => x.Id);
                    table.CheckConstraint("CK_ThemeWebTemplatesMapping_ContentItemId_ViewId", "[ContentItemId] IS NOT NULL OR [ViewId] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_ThemeWebTemplatesMappings_ContentItems_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThemeWebTemplatesMappings_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThemeWebTemplatesMappings_Views_ViewId",
                        column: x => x.ViewId,
                        principalTable: "Views",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThemeWebTemplatesMappings_WebTemplates_WebTemplateId",
                        column: x => x.WebTemplateId,
                        principalTable: "WebTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplates_DeveloperName",
                table: "WebTemplates",
                column: "DeveloperName")
                .Annotation("SqlServer:Include", new[] { "Id", "Label" });

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplates_DeveloperName_ThemeId",
                table: "WebTemplates",
                columns: new[] { "DeveloperName", "ThemeId" },
                unique: true,
                filter: "[DeveloperName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplates_ThemeId",
                table: "WebTemplates",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeAccessToMediaItems_MediaItemId",
                table: "ThemeAccessToMediaItems",
                column: "MediaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeAccessToMediaItems_ThemeId",
                table: "ThemeAccessToMediaItems",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeRevisions_CreatorUserId",
                table: "ThemeRevisions",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeRevisions_LastModifierUserId",
                table: "ThemeRevisions",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeRevisions_ThemeId",
                table: "ThemeRevisions",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Themes_CreatorUserId",
                table: "Themes",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Themes_DeveloperName",
                table: "Themes",
                column: "DeveloperName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_LastModifierUserId",
                table: "Themes",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Themes_PreviewImageId",
                table: "Themes",
                column: "PreviewImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeWebTemplatesMappings_ContentItemId",
                table: "ThemeWebTemplatesMappings",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeWebTemplatesMappings_ThemeId",
                table: "ThemeWebTemplatesMappings",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeWebTemplatesMappings_ViewId",
                table: "ThemeWebTemplatesMappings",
                column: "ViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeWebTemplatesMappings_WebTemplateId",
                table: "ThemeWebTemplatesMappings",
                column: "WebTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_WebTemplates_WebTemplateId",
                table: "ContentItems",
                column: "WebTemplateId",
                principalTable: "WebTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Views_WebTemplates_WebTemplateId",
                table: "Views",
                column: "WebTemplateId",
                principalTable: "WebTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WebTemplates_Themes_ThemeId",
                table: "WebTemplates",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_WebTemplates_WebTemplateId",
                table: "ContentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Views_WebTemplates_WebTemplateId",
                table: "Views");

            migrationBuilder.DropForeignKey(
                name: "FK_WebTemplates_Themes_ThemeId",
                table: "WebTemplates");

            migrationBuilder.DropTable(
                name: "ThemeAccessToMediaItems");

            migrationBuilder.DropTable(
                name: "ThemeRevisions");

            migrationBuilder.DropTable(
                name: "ThemeWebTemplatesMappings");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_WebTemplates_DeveloperName",
                table: "WebTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WebTemplates_DeveloperName_ThemeId",
                table: "WebTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WebTemplates_ThemeId",
                table: "WebTemplates");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "WebTemplates");

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplates_DeveloperName",
                table: "WebTemplates",
                column: "DeveloperName",
                unique: true,
                filter: "[DeveloperName] IS NOT NULL")
                .Annotation("SqlServer:Include", new[] { "Id", "Label" });

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_WebTemplates_WebTemplateId",
                table: "ContentItems",
                column: "WebTemplateId",
                principalTable: "WebTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Views_WebTemplates_WebTemplateId",
                table: "Views",
                column: "WebTemplateId",
                principalTable: "WebTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
