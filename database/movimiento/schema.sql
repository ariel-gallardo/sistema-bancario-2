DECLARE @DbName sysname = N'MovimientosDb';
IF DB_ID(@DbName) IS NULL
BEGIN
    EXEC('CREATE DATABASE [' + @DbName + N']');
END;
GO

USE [MovimientoDb];
GO

IF OBJECT_ID('dbo.Movimientos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Movimientos
    (
        Id INT NOT NULL PRIMARY KEY,
        TarjetaId INT NOT NULL,
        Fecha DATE NOT NULL,
        Monto DECIMAL(18,2) NOT NULL,
        Descripcion NVARCHAR(200) NOT NULL
    );
END;
GO

MERGE dbo.Movimientos AS target
USING (VALUES
    (1, 1001, '2024-04-20', -9500.00, 'Compra supermercado'),
    (2, 1001, '2024-04-18', -12000.00, 'Pago seguro auto'),
    (3, 1001, '2024-04-15', -6500.00, 'Farmacia'),
    (4, 1001, '2024-04-12', -43000.00, 'Electrodomestico'),
    (5, 1001, '2024-04-10', -1999.00, 'Spotify Premium'),
    (6, 1001, '2024-03-28', -1500.00, 'Carga SUBE'),
    (7, 1002, '2024-04-05', -3000.00, 'Compra secundaria'),
    (8, 2001, '2024-04-19', -7500.00, 'Cena en restaurante'),
    (9, 2001, '2024-04-17', -3000.00, 'Pago Netflix'),
    (10, 2001, '2024-04-14', -18000.00, 'Compra electro'),
    (11, 2001, '2024-04-11', -15000.00, 'Ropa'),
    (12, 2001, '2024-04-09', -2500.00, 'Heladeria')
) AS source (Id, TarjetaId, Fecha, Monto, Descripcion)
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET TarjetaId = source.TarjetaId, Fecha = source.Fecha, Monto = source.Monto, Descripcion = source.Descripcion
WHEN NOT MATCHED THEN
    INSERT (Id, TarjetaId, Fecha, Monto, Descripcion)
    VALUES (source.Id, source.TarjetaId, source.Fecha, source.Monto, source.Descripcion);
GO

USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'movimientos_app')
BEGIN
    CREATE LOGIN [movimientos_app] WITH PASSWORD = N'Movimiento#2026';
END;
GO
USE [MovimientoDb];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'movimientos_app')
BEGIN
    CREATE USER [movimientos_app] FOR LOGIN [movimientos_app];
    ALTER ROLE db_owner ADD MEMBER [movimientos_app];
END;
GO
