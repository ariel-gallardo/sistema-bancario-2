DECLARE @DbName sysname = N'ClienteCuentaDb';
IF DB_ID(@DbName) IS NULL
BEGIN
    EXEC('CREATE DATABASE [' + @DbName + N']');
END;
GO

USE [ClienteCuentaDb];
GO

IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id INT NOT NULL PRIMARY KEY,
        Nombre NVARCHAR(200) NOT NULL
    );
END;
GO

IF OBJECT_ID('dbo.Cuentas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cuentas
    (
        Id INT NOT NULL PRIMARY KEY,
        ClienteId INT NOT NULL,
        Saldo DECIMAL(18,2) NOT NULL,
        EsCuentaPrincipal BIT NOT NULL DEFAULT(0)
    );
END;
GO

IF COL_LENGTH('dbo.Cuentas', 'EsActiva') IS NULL
BEGIN
    ALTER TABLE dbo.Cuentas
    ADD EsActiva BIT NOT NULL CONSTRAINT DF_Cuentas_EsActiva DEFAULT(1) WITH VALUES;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Cuentas_Cliente_Principal'
      AND object_id = OBJECT_ID('dbo.Cuentas')
)
BEGIN
    DROP INDEX IX_Cuentas_Cliente_Principal ON dbo.Cuentas;
END;
GO

CREATE NONCLUSTERED INDEX IX_Cuentas_Cliente_Principal
    ON dbo.Cuentas (ClienteId, EsCuentaPrincipal, EsActiva)
    INCLUDE (Saldo);
GO

MERGE dbo.Clientes AS target
USING (VALUES
    (1, 'María González'),
    (2, 'Juan Pérez')
) AS source (Id, Nombre)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Nombre = source.Nombre
WHEN NOT MATCHED THEN INSERT (Id, Nombre) VALUES (source.Id, source.Nombre);
GO

MERGE dbo.Cuentas AS target
USING (VALUES
    (101, 1, 157500.25, 1, 1),
    (102, 1, 25300.75, 0, 1),
    (201, 2, 80200.00, 1, 1)
) AS source (Id, ClienteId, Saldo, EsCuentaPrincipal, EsActiva)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET ClienteId = source.ClienteId, Saldo = source.Saldo, EsCuentaPrincipal = source.EsCuentaPrincipal, EsActiva = source.EsActiva
WHEN NOT MATCHED THEN INSERT (Id, ClienteId, Saldo, EsCuentaPrincipal, EsActiva)
VALUES (source.Id, source.ClienteId, source.Saldo, source.EsCuentaPrincipal, source.EsActiva);
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.usp_GetSaldoCuentaPrincipal
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        c.Id AS CuentaId,
        c.ClienteId,
        c.Saldo AS SaldoCuentaPrincipal,
        CASE
            WHEN EXISTS (
                SELECT 1
                FROM TarjetaDb.dbo.Tarjetas AS t WITH (NOLOCK)
                WHERE t.CuentaId = c.Id
                  AND t.EsPrincipal = 1
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS TieneTarjetaPrincipal
    FROM dbo.Cuentas AS c WITH (NOLOCK)
    WHERE c.ClienteId = @ClienteId
            AND c.EsCuentaPrincipal = 1
            AND c.EsActiva = 1
    ORDER BY c.Id DESC;
END;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.usp_GetClienteResumen
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CuentaPrincipalId INT = (
        SELECT TOP 1 Id
        FROM dbo.Cuentas
        WHERE ClienteId = @ClienteId AND EsCuentaPrincipal = 1 AND EsActiva = 1
        ORDER BY Id DESC
    );

    IF @CuentaPrincipalId IS NULL
    BEGIN
        SELECT CAST(NULL AS DECIMAL(18,2)) AS SaldoCuentaPrincipal WHERE 1 = 0;
        SELECT CAST(NULL AS DATE) AS Fecha, CAST(0 AS DECIMAL(18,2)) AS Monto, CAST('Sin tarjeta principal' AS NVARCHAR(200)) AS Descripcion WHERE 1 = 0;
        RETURN;
    END;

    SELECT Saldo AS SaldoCuentaPrincipal
    FROM dbo.Cuentas
    WHERE Id = @CuentaPrincipalId AND EsActiva = 1;

    DECLARE @TarjetaPrincipalId INT = (
        SELECT TOP 1 t.Id
        FROM TarjetaDb.dbo.Tarjetas t
        WHERE t.CuentaId = @CuentaPrincipalId AND t.EsPrincipal = 1
        ORDER BY t.Id DESC
    );

    IF @TarjetaPrincipalId IS NULL
    BEGIN
        SELECT CAST(NULL AS DATE) AS Fecha, CAST(0 AS DECIMAL(18,2)) AS Monto, CAST('Sin tarjeta principal' AS NVARCHAR(200)) AS Descripcion WHERE 1 = 0;
        RETURN;
    END;

    SELECT TOP 5 Fecha, Monto, Descripcion
    FROM MovimientoDb.dbo.Movimientos
    WHERE TarjetaId = @TarjetaPrincipalId
    ORDER BY Fecha DESC;
END;
GO

USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'clientes_app')
BEGIN
    CREATE LOGIN [clientes_app] WITH PASSWORD = N'Cliente#2026';
END;
GO
USE [ClienteCuentaDb];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'clientes_app')
BEGIN
    CREATE USER [clientes_app] FOR LOGIN [clientes_app];
    ALTER ROLE db_owner ADD MEMBER [clientes_app];
END;
GO
