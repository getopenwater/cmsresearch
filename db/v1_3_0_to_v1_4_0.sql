BEGIN TRANSACTION;
GO

ALTER TABLE [ContentItems] DROP CONSTRAINT [FK_ContentItems_WebTemplates_WebTemplateId];
GO

ALTER TABLE [Views] DROP CONSTRAINT [FK_Views_WebTemplates_WebTemplateId];
GO

DROP INDEX [IX_WebTemplates_DeveloperName] ON [WebTemplates];
GO

CREATE TABLE [Themes] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [DeveloperName] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [IsCanExport] bit NOT NULL,
    [PreviewImageId] uniqueidentifier NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    [CreatorUserId] uniqueidentifier NULL,
    [LastModifierUserId] uniqueidentifier NULL,
    CONSTRAINT [PK_Themes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Themes_MediaItems_PreviewImageId] FOREIGN KEY ([PreviewImageId]) REFERENCES [MediaItems] ([Id]),
    CONSTRAINT [FK_Themes_Users_CreatorUserId] FOREIGN KEY ([CreatorUserId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Themes_Users_LastModifierUserId] FOREIGN KEY ([LastModifierUserId]) REFERENCES [Users] ([Id])
);
GO

ALTER TABLE [WebTemplates] ADD [ThemeId] uniqueidentifier NOT NULL DEFAULT '9b6bb2ec-6652-4303-aded-5755cbe580a8';
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Title', N'DeveloperName', N'IsActive', N'IsCanExport', N'Description', N'CreationTime') AND [object_id] = OBJECT_ID(N'[Themes]'))
    SET IDENTITY_INSERT [Themes] ON;
INSERT INTO [Themes] ([Id], [Title], [DeveloperName], [IsActive], [IsCanExport], [Description], [CreationTime])
VALUES ('9b6bb2ec-6652-4303-aded-5755cbe580a8', N'Raytha default theme', N'raytha_default_theme', CAST(1 AS bit), CAST(0 AS bit), N'Raytha default theme', '2024-07-11T18:19:58.3285030Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Title', N'DeveloperName', N'IsActive', N'IsCanExport', N'Description', N'CreationTime') AND [object_id] = OBJECT_ID(N'[Themes]'))
    SET IDENTITY_INSERT [Themes] OFF;
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

CREATE TABLE [ThemeRevisions] (
    [Id] uniqueidentifier NOT NULL,
    [ThemeId] uniqueidentifier NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [WebTemplatesJson] nvarchar(max) NOT NULL,
    [WebTemplatesMappingJson] nvarchar(max) NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [LastModificationTime] datetime2 NULL,
    [CreatorUserId] uniqueidentifier NULL,
    [LastModifierUserId] uniqueidentifier NULL,
    CONSTRAINT [PK_ThemeRevisions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThemeRevisions_Themes_ThemeId] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ThemeRevisions_Users_CreatorUserId] FOREIGN KEY ([CreatorUserId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_ThemeRevisions_Users_LastModifierUserId] FOREIGN KEY ([LastModifierUserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [ThemeWebTemplatesMappings] (
    [Id] uniqueidentifier NOT NULL,
    [WebTemplateId] uniqueidentifier NOT NULL,
    [ThemeId] uniqueidentifier NOT NULL,
    [ContentItemId] uniqueidentifier NULL,
    [ViewId] uniqueidentifier NULL,
    CONSTRAINT [PK_ThemeWebTemplatesMappings] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ThemeWebTemplatesMapping_ContentItemId_ViewId] CHECK ([ContentItemId] IS NOT NULL OR [ViewId] IS NOT NULL),
    CONSTRAINT [FK_ThemeWebTemplatesMappings_ContentItems_ContentItemId] FOREIGN KEY ([ContentItemId]) REFERENCES [ContentItems] ([Id]),
    CONSTRAINT [FK_ThemeWebTemplatesMappings_Themes_ThemeId] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ThemeWebTemplatesMappings_Views_ViewId] FOREIGN KEY ([ViewId]) REFERENCES [Views] ([Id]),
    CONSTRAINT [FK_ThemeWebTemplatesMappings_WebTemplates_WebTemplateId] FOREIGN KEY ([WebTemplateId]) REFERENCES [WebTemplates] ([Id]) ON DELETE CASCADE
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

CREATE INDEX [IX_ThemeRevisions_CreatorUserId] ON [ThemeRevisions] ([CreatorUserId]);
GO

CREATE INDEX [IX_ThemeRevisions_LastModifierUserId] ON [ThemeRevisions] ([LastModifierUserId]);
GO

CREATE INDEX [IX_ThemeRevisions_ThemeId] ON [ThemeRevisions] ([ThemeId]);
GO

CREATE INDEX [IX_Themes_CreatorUserId] ON [Themes] ([CreatorUserId]);
GO

CREATE UNIQUE INDEX [IX_Themes_DeveloperName] ON [Themes] ([DeveloperName]);
GO

CREATE INDEX [IX_Themes_LastModifierUserId] ON [Themes] ([LastModifierUserId]);
GO

CREATE INDEX [IX_Themes_PreviewImageId] ON [Themes] ([PreviewImageId]);
GO

CREATE INDEX [IX_ThemeWebTemplatesMappings_ContentItemId] ON [ThemeWebTemplatesMappings] ([ContentItemId]);
GO

CREATE INDEX [IX_ThemeWebTemplatesMappings_ThemeId] ON [ThemeWebTemplatesMappings] ([ThemeId]);
GO

CREATE INDEX [IX_ThemeWebTemplatesMappings_ViewId] ON [ThemeWebTemplatesMappings] ([ViewId]);
GO

CREATE INDEX [IX_ThemeWebTemplatesMappings_WebTemplateId] ON [ThemeWebTemplatesMappings] ([WebTemplateId]);
GO

ALTER TABLE [ContentItems] ADD CONSTRAINT [FK_ContentItems_WebTemplates_WebTemplateId] FOREIGN KEY ([WebTemplateId]) REFERENCES [WebTemplates] ([Id]);
GO

ALTER TABLE [Views] ADD CONSTRAINT [FK_Views_WebTemplates_WebTemplateId] FOREIGN KEY ([WebTemplateId]) REFERENCES [WebTemplates] ([Id]);
GO

ALTER TABLE [WebTemplates] ADD CONSTRAINT [FK_WebTemplates_Themes_ThemeId] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240711180014_v1_4_0', N'8.0.0');
GO

COMMIT;
GO

