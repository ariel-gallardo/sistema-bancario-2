DECLARE @DbName sysname = N'TarjetaDb';
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
        ClienteId INT NOT NULL,
        Numero NVARCHAR(32) NOT NULL,
        EsPrincipal BIT NOT NULL DEFAULT(0)
    );
END;
GO

IF COL_LENGTH('dbo.Tarjetas', 'ClienteId') IS NULL
BEGIN
    ALTER TABLE dbo.Tarjetas
    ADD ClienteId INT NOT NULL CONSTRAINT DF_Tarjetas_ClienteId DEFAULT(0) WITH VALUES;
END;
GO

IF EXISTS (SELECT 1 FROM ClienteCuentaDb.sys.tables WHERE name = 'Cuentas')
BEGIN
    UPDATE t
        SET ClienteId = c.ClienteId
    FROM dbo.Tarjetas AS t
    INNER JOIN ClienteCuentaDb.dbo.Cuentas AS c ON c.Id = t.CuentaId
    WHERE t.ClienteId = 0;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Tarjetas_Cuenta_Principal'
      AND object_id = OBJECT_ID('dbo.Tarjetas')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tarjetas_Cuenta_Principal
        ON dbo.Tarjetas (CuentaId, EsPrincipal)
        INCLUDE (Numero);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Tarjetas_Cliente_Principal'
      AND object_id = OBJECT_ID('dbo.Tarjetas')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tarjetas_Cliente_Principal
        ON dbo.Tarjetas (ClienteId, EsPrincipal)
        INCLUDE (Numero, CuentaId);
END;
GO

MERGE dbo.Tarjetas AS target
USING (VALUES
    (1001, 101, 1, '4509-8701-2345-6789', 1),
    (1002, 101, 1, '4509-8701-2345-6790', 0),
    (2001, 201, 2, '4510-2301-3345-1244', 1)
) AS source (Id, CuentaId, ClienteId, Numero, EsPrincipal)
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET CuentaId = source.CuentaId, ClienteId = source.ClienteId, Numero = source.Numero, EsPrincipal = source.EsPrincipal
WHEN NOT MATCHED THEN
    INSERT (Id, CuentaId, ClienteId, Numero, EsPrincipal)
    VALUES (source.Id, source.CuentaId, source.ClienteId, source.Numero, source.EsPrincipal);
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
