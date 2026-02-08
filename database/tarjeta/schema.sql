DECLARE @DbName sysname = N'TarjetasDb';
IF DB_ID(@DbName) IS NULL
BEGIN
    EXEC('CREATE DATABASE [' + @DbName + N']');
END;
GO

USE [TarjetaDb];
GO

IF OBJECT_ID('dbo.Tarjetas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tarjetas
    (
        Id INT NOT NULL PRIMARY KEY,
        CuentaId INT NOT NULL,
        Numero NVARCHAR(32) NOT NULL,
        EsPrincipal BIT NOT NULL DEFAULT(0)
    );
END;
GO

MERGE dbo.Tarjetas AS target
USING (VALUES
    (1001, 101, '4509-8701-2345-6789', 1),
    (1002, 101, '4509-8701-2345-6790', 0),
    (2001, 201, '4510-2301-3345-1244', 1)
) AS source (Id, CuentaId, Numero, EsPrincipal)
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET CuentaId = source.CuentaId, Numero = source.Numero, EsPrincipal = source.EsPrincipal
WHEN NOT MATCHED THEN
    INSERT (Id, CuentaId, Numero, EsPrincipal)
    VALUES (source.Id, source.CuentaId, source.Numero, source.EsPrincipal);
GO

USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'tarjeta_app')
BEGIN
    CREATE LOGIN [tarjeta_app] WITH PASSWORD = N'Tarjeta#2026';
END;
GO
USE [TarjetaDb];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'tarjeta_app')
BEGIN
    CREATE USER [tarjeta_app] FOR LOGIN [tarjeta_app];
    ALTER ROLE db_owner ADD MEMBER [tarjeta_app];
END;
GO
