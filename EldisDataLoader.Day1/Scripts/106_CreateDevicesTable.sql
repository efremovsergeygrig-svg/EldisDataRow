SELECT
        *
    FROM
        [dbo].[EldisDevices]

REturn


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EldisDevices')
BEGIN
    CREATE TABLE [dbo].[EldisDevices]
    (
        [LINK]                          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Id]                            UNIQUEIDENTIFIER NOT NULL UNIQUE,
        [ModemId]                       UNIQUEIDENTIFIER NULL,
        [ObjectId]                      UNIQUEIDENTIFIER NULL,
        [DeviceStatus]                  INT NULL,
        [ModemStatus]                   INT NULL,
        [ModemSubStatus]                INT NULL,
        [LastConnection]                DATETIME2 NULL,
        [LastConnectionStatusColor]     NVARCHAR(50) NULL,
        [ModemIsBusy]                   BIT NULL,
        [DeviceModel]                   NVARCHAR(255) NULL,
        [DeviceName]                    NVARCHAR(255) NULL,
        [AddressObject]                 NVARCHAR(500) NULL,
        [ObjectName]                    NVARCHAR(255) NULL,
        [Consumer]                      NVARCHAR(255) NULL,
        [ModemName]                     NVARCHAR(255) NULL,
        [TimeOnDevice]                  NVARCHAR(255) NULL,
        [ImpossibleGetExactTime]        BIT NULL,
        [Description]                   NVARCHAR(MAX) NULL,
        [DashboardId]                   UNIQUEIDENTIFIER NULL,
        [DashboardName]                 NVARCHAR(255) NULL,
        [CreatedOn]                     DATETIME2 NULL,
        [Latitude]                      FLOAT NULL,
        [Longitude]                     FLOAT NULL,
        [ImportedAt]                    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO