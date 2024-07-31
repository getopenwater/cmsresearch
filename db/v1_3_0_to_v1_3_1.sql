BEGIN TRANSACTION;
GO

DROP INDEX [IX_WebTemplates_DeveloperName] ON [WebTemplates];
GO

CREATE TABLE [Themes] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [DeveloperName] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsExportable] bit NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    [CreatorUserId] uniqueidentifier NULL,
    [LastModifierUserId] uniqueidentifier NULL,
    CONSTRAINT [PK_Themes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Themes_Users_CreatorUserId] FOREIGN KEY ([CreatorUserId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Themes_Users_LastModifierUserId] FOREIGN KEY ([LastModifierUserId]) REFERENCES [Users] ([Id])
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Title', N'DeveloperName', N'IsExportable', N'Description', N'CreationTime') AND [object_id] = OBJECT_ID(N'[Themes]'))
    SET IDENTITY_INSERT [Themes] ON;
INSERT INTO [Themes] ([Id], [Title], [DeveloperName], [IsExportable], [Description], [CreationTime])
VALUES ('154d3ad5-7949-4b2c-8446-5f1d7333a9ee', N'Raytha default theme', N'raytha_default_theme', CAST(0 AS bit), N'Raytha default theme', '2024-07-31T05:47:22.9976135Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Title', N'DeveloperName', N'IsExportable', N'Description', N'CreationTime') AND [object_id] = OBJECT_ID(N'[Themes]'))
    SET IDENTITY_INSERT [Themes] OFF;
GO

ALTER TABLE [WebTemplates] ADD [ThemeId] uniqueidentifier NOT NULL DEFAULT '154d3ad5-7949-4b2c-8446-5f1d7333a9ee';
GO

ALTER TABLE [OrganizationSettings] ADD [ActiveThemeId] uniqueidentifier NOT NULL DEFAULT '154d3ad5-7949-4b2c-8446-5f1d7333a9ee';
GO

CREATE TABLE [ThemeWebTemplateContentItemMappings] (
    [Id] uniqueidentifier NOT NULL,
    [WebTemplateId] uniqueidentifier NOT NULL,
    [ThemeId] uniqueidentifier NOT NULL,
    [ContentItemId] uniqueidentifier NULL,
    CONSTRAINT [PK_ThemeWebTemplateContentItemMappings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThemeWebTemplateContentItemMappings_ContentItems_ContentItemId] FOREIGN KEY ([ContentItemId]) REFERENCES [ContentItems] ([Id]),
    CONSTRAINT [FK_ThemeWebTemplateContentItemMappings_WebTemplates_WebTemplateId] FOREIGN KEY ([WebTemplateId]) REFERENCES [WebTemplates] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ThemeWebTemplateViewMappings] (
    [Id] uniqueidentifier NOT NULL,
    [WebTemplateId] uniqueidentifier NOT NULL,
    [ThemeId] uniqueidentifier NOT NULL,
    [ViewId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ThemeWebTemplateViewMappings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThemeWebTemplateViewMappings_Views_ViewId] FOREIGN KEY ([ViewId]) REFERENCES [Views] ([Id]),
    CONSTRAINT [FK_ThemeWebTemplateViewMappings_WebTemplates_WebTemplateId] FOREIGN KEY ([WebTemplateId]) REFERENCES [WebTemplates] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ThemeAccessToMediaItems] (
    [Id] uniqueidentifier NOT NULL,
    [ThemeId] uniqueidentifier NOT NULL,
    [MediaItemId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ThemeAccessToMediaItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThemeAccessToMediaItems_MediaItems_MediaItemId] FOREIGN KEY ([MediaItemId]) REFERENCES [MediaItems] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ThemeAccessToMediaItems_Themes_ThemeId] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_WebTemplates_DeveloperName] ON [WebTemplates] ([DeveloperName]) INCLUDE ([Id], [Label]);
GO

CREATE UNIQUE INDEX [IX_WebTemplates_DeveloperName_ThemeId] ON [WebTemplates] ([DeveloperName], [ThemeId]) WHERE [DeveloperName] IS NOT NULL;
GO

CREATE INDEX [IX_WebTemplates_ThemeId] ON [WebTemplates] ([ThemeId]);
GO

CREATE INDEX [IX_ThemeAccessToMediaItems_MediaItemId] ON [ThemeAccessToMediaItems] ([MediaItemId]);
GO

CREATE INDEX [IX_ThemeAccessToMediaItems_ThemeId] ON [ThemeAccessToMediaItems] ([ThemeId]);
GO

CREATE INDEX [IX_Themes_CreatorUserId] ON [Themes] ([CreatorUserId]);
GO

CREATE UNIQUE INDEX [IX_Themes_DeveloperName] ON [Themes] ([DeveloperName]);
GO

CREATE INDEX [IX_Themes_LastModifierUserId] ON [Themes] ([LastModifierUserId]);
GO

CREATE INDEX [IX_ThemeWebTemplateContentItemMappings_ContentItemId] ON [ThemeWebTemplateContentItemMappings] ([ContentItemId]);
GO

CREATE UNIQUE INDEX [IX_ThemeWebTemplateContentItemMappings_ThemeId_WebTemplateId_ContentItemId] ON [ThemeWebTemplateContentItemMappings] ([ThemeId], [WebTemplateId], [ContentItemId]) WHERE [ContentItemId] IS NOT NULL;
GO

CREATE INDEX [IX_ThemeWebTemplateContentItemMappings_WebTemplateId] ON [ThemeWebTemplateContentItemMappings] ([WebTemplateId]);
GO

CREATE UNIQUE INDEX [IX_ThemeWebTemplateViewMappings_ThemeId_ViewId_WebTemplateId] ON [ThemeWebTemplateViewMappings] ([ThemeId], [ViewId], [WebTemplateId]);
GO

CREATE INDEX [IX_ThemeWebTemplateViewMappings_ViewId] ON [ThemeWebTemplateViewMappings] ([ViewId]);
GO

CREATE INDEX [IX_ThemeWebTemplateViewMappings_WebTemplateId] ON [ThemeWebTemplateViewMappings] ([WebTemplateId]);
GO

ALTER TABLE [WebTemplates] ADD CONSTRAINT [FK_WebTemplates_Themes_ThemeId] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id]) ON DELETE CASCADE;
GO


                INSERT INTO ThemeWebTemplateViewMappings (Id, WebTemplateId, ThemeId, ViewId)
                SELECT NEWID(), WebTemplateId, (SELECT ActiveThemeId FROM OrganizationSettings), Id
                FROM Views
                WHERE WebTemplateId IS NOT NULL
            
GO


                INSERT INTO ThemeWebTemplateContentItemMappings (Id, WebTemplateId, ThemeId, ContentItemId)
                SELECT NEWID(), WebTemplateId, (SELECT ActiveThemeId FROM OrganizationSettings), Id
                FROM ContentItems
                WHERE Id IS NOT NULL
            
GO

ALTER TABLE [ContentItems] DROP CONSTRAINT [FK_ContentItems_WebTemplates_WebTemplateId];
GO

ALTER TABLE [Views] DROP CONSTRAINT [FK_Views_WebTemplates_WebTemplateId];
GO

DROP INDEX [IX_Views_WebTemplateId] ON [Views];
GO

DROP INDEX [IX_ContentItems_WebTemplateId] ON [ContentItems];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Views]') AND [c].[name] = N'WebTemplateId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Views] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Views] DROP COLUMN [WebTemplateId];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContentItems]') AND [c].[name] = N'WebTemplateId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ContentItems] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [ContentItems] DROP COLUMN [WebTemplateId];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240731050129_v1_3_1', N'8.0.3');
GO

COMMIT;
GO

