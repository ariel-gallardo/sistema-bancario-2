DECLARE @DbName sysname = N'LoggingDb';
IF DB_ID(@DbName) IS NULL
BEGIN
    EXEC('CREATE DATABASE [' + @DbName + N']');
END;
GO

USE [LoggingDb];
GO

IF OBJECT_ID('dbo.Logs', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Logs;
END;
GO

CREATE TABLE dbo.Logs
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TimestampUtc DATETIME2(3) NOT NULL,
    ServiceName NVARCHAR(120) NOT NULL,
    Message NVARCHAR(1024) NOT NULL,
    Severity NVARCHAR(40) NOT NULL,
    RequestPath NVARCHAR(400) NULL,
    StackTrace NVARCHAR(MAX) NULL,
    CorrelationId NVARCHAR(100) NULL,
    Payload NVARCHAR(MAX) NULL
);
GO

CREATE NONCLUSTERED INDEX IX_Logs_ServiceName
    ON dbo.Logs (ServiceName, TimestampUtc DESC);
GO

CREATE NONCLUSTERED INDEX IX_Logs_Severity
    ON dbo.Logs (Severity, TimestampUtc DESC);
GO

CREATE NONCLUSTERED INDEX IX_Logs_CorrelationId
    ON dbo.Logs (CorrelationId)
    WHERE CorrelationId IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX IX_Logs_RequestPath
    ON dbo.Logs (RequestPath)
    WHERE RequestPath IS NOT NULL;
GO

USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'logging_app')
BEGIN
    CREATE LOGIN [logging_app] WITH PASSWORD = N'Logging#2026';
END;
GO
USE [LoggingDb];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'logging_app')
BEGIN
    CREATE USER [logging_app] FOR LOGIN [logging_app];
    ALTER ROLE db_owner ADD MEMBER [logging_app];
END;
GO
