BEGIN TRANSACTION;
GO


                UPDATE ContentTypeFields
                SET FieldType = 'wysiwyg'
                WHERE FieldType = 'long_text';
            
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241102184009_v1_4_0', N'8.0.0');
GO

COMMIT;
GO

