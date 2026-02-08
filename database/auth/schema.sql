DECLARE @DbName sysname = N'AuthDb';
IF DB_ID(@DbName) IS NULL
BEGIN
    EXEC('CREATE DATABASE [' + @DbName + N']');
END;
GO

USE [AuthDb];
GO

IF OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Usuarios;
END;
GO

CREATE TABLE dbo.Usuarios
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username NVARCHAR(120) NOT NULL UNIQUE,
    PasswordHash VARBINARY(32) NOT NULL,
    Roles NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT(1),
    ClienteId INT NULL
);
GO

MERGE dbo.Usuarios AS target
USING (VALUES
    ('maria', HASHBYTES('SHA2_256', 'P@ssw0rd'), 'cliente,vip', 1, 1),
    ('juan', HASHBYTES('SHA2_256', 'P@ssw0rd'), 'cliente', 1, 2),
    ('admin', HASHBYTES('SHA2_256', 'Admin2024'), 'admin', 1, NULL),
    ('tester1', HASHBYTES('SHA2_256', 'Tester#2026'), 'cliente', 1, NULL),
    ('tester2', HASHBYTES('SHA2_256', 'Tester#2026'), 'cliente,premium', 1, NULL)
) AS source (Username, PasswordHash, Roles, IsActive, ClienteId)
ON target.Username = source.Username
WHEN MATCHED THEN
    UPDATE SET PasswordHash = source.PasswordHash, Roles = source.Roles, IsActive = source.IsActive, ClienteId = source.ClienteId
WHEN NOT MATCHED THEN
    INSERT (Username, PasswordHash, Roles, IsActive, ClienteId)
    VALUES (source.Username, source.PasswordHash, source.Roles, source.IsActive, source.ClienteId);
GO

USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'auth_app')
BEGIN
    CREATE LOGIN [auth_app] WITH PASSWORD = N'Auth#2026';
END;
GO
USE [AuthDb];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'auth_app')
BEGIN
    CREATE USER [auth_app] FOR LOGIN [auth_app];
    ALTER ROLE db_owner ADD MEMBER [auth_app];
END;
GO
