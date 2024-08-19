﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Raytha.Domain.Entities;

#nullable disable

namespace Raytha.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class v1_3_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WebTemplates_DeveloperName",
                table: "WebTemplates");

            migrationBuilder.AddColumn<string>(
                name: "WebTemplateIdsJson",
                table: "DeletedContentItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql(@"
                UPDATE DeletedContentItems
                SET WebTemplateIdsJson = '[""' + CAST(WebTemplateId AS NVARCHAR(36)) + '""]'
            ");

            migrationBuilder.DropColumn(
                name: "WebTemplateId",
                table: "DeletedContentItems");

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeveloperName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsExportable = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifierUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
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

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Title", "DeveloperName", "IsExportable", "Description", "CreationTime" },
                values: new object[,]
                {
                    { defaultThemeId, "Raytha default theme", Theme.DEFAULT_THEME_DEVELOPER_NAME, false, "Raytha default theme", DateTime.UtcNow },
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ThemeId",
                table: "WebTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: defaultThemeId);

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveThemeId",
                table: "OrganizationSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: defaultThemeId);

            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_WebTemplates_WebTemplateId",
                table: "ContentItems");

            migrationBuilder.DropIndex(
                name: "IX_ContentItems_WebTemplateId",
                table: "ContentItems");

            migrationBuilder.CreateTable(
                name: "WebTemplateContentItemRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebTemplateContentItemRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebTemplateContentItemRelations_ContentItems_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebTemplateContentItemRelations_WebTemplates_WebTemplateId",
                        column: x => x.WebTemplateId,
                        principalTable: "WebTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO WebTemplateContentItemRelations (Id, WebTemplateId, ContentItemId)
                SELECT NEWID(), WebTemplateId, Id
                FROM ContentItems
            ");

            migrationBuilder.DropColumn(
                name: "WebTemplateId",
                table: "ContentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Views_WebTemplates_WebTemplateId",
                table: "Views");

            migrationBuilder.DropIndex(
                name: "IX_Views_WebTemplateId",
                table: "Views");

            migrationBuilder.CreateTable(
                name: "WebTemplateViewRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebTemplateViewRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebTemplateViewRelations_Views_ViewId",
                        column: x => x.ViewId,
                        principalTable: "Views",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebTemplateViewRelations_WebTemplates_WebTemplateId",
                        column: x => x.WebTemplateId,
                        principalTable: "WebTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO WebTemplateViewRelations (Id, WebTemplateId, ViewId)
                SELECT NEWID(), WebTemplateId, Id
                FROM Views
            ");

            migrationBuilder.DropColumn(
                name: "WebTemplateId",
                table: "Views");

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

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplates_DeveloperName_ThemeId",
                table: "WebTemplates",
                columns: new[] { "DeveloperName", "ThemeId" },
                unique: true);

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
                name: "IX_WebTemplateContentItemRelations_ContentItemId",
                table: "WebTemplateContentItemRelations",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplateContentItemRelations_WebTemplateId_ContentItemId",
                table: "WebTemplateContentItemRelations",
                columns: new[] { "WebTemplateId", "ContentItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplateViewRelations_ViewId_WebTemplateId",
                table: "WebTemplateViewRelations",
                columns: new[] { "ViewId", "WebTemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebTemplateViewRelations_WebTemplateId",
                table: "WebTemplateViewRelations",
                column: "WebTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebTemplates_Themes_ThemeId",
                table: "WebTemplates",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebTemplates_Themes_ThemeId",
                table: "WebTemplates");

            migrationBuilder.DropTable(
                name: "ThemeAccessToMediaItems");

            migrationBuilder.DropTable(
                name: "WebTemplateContentItemRelations");

            migrationBuilder.DropTable(
                name: "WebTemplateViewRelations");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_WebTemplates_DeveloperName_ThemeId",
                table: "WebTemplates");

            migrationBuilder.DropIndex(
                name: "IX_WebTemplates_ThemeId",
                table: "WebTemplates");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "WebTemplates");

            migrationBuilder.DropColumn(
                name: "ActiveThemeId",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "WebTemplateIdsJson",
                table: "DeletedContentItems");

            migrationBuilder.AddColumn<Guid>(
                name: "WebTemplateId",
                table: "Views",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WebTemplateId",
                table: "DeletedContentItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WebTemplateId",
                table: "ContentItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Views_WebTemplateId",
                table: "Views",
                column: "WebTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_WebTemplateId",
                table: "ContentItems",
                column: "WebTemplateId");

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
