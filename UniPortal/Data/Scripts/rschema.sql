sUSE [UniPortalDB];
GO

-- 1️⃣ Drop all foreign key constraints
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += '
ALTER TABLE [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] DROP CONSTRAINT [' + fk.name + '];'
FROM sys.foreign_keys fk
JOIN sys.tables t ON fk.parent_object_id = t.object_id;

EXEC sp_executesql @sql;

-- 2️⃣ Drop all user tables
SET @sql = '';

SELECT @sql += '
DROP TABLE [' + SCHEMA_NAME(schema_id) + '].[' + name + '];'
FROM sys.tables;

EXEC sp_executesql @sql;
