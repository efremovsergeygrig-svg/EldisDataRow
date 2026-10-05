IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EldisObjects')
BEGIN
    CREATE TABLE [dbo].[EldisObjects]
    (
        [LINK]                          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Id]                            UNIQUEIDENTIFIER NOT NULL UNIQUE,
        [IsWinterMode]                  BIT NOT NULL DEFAULT 0,
        [IsModeChanged]                 BIT NOT NULL DEFAULT 0,
        [Address]                       NVARCHAR(500) NULL,
        [TypeObjectId]                  UNIQUEIDENTIFIER NULL,
        [TypeObjectName]                NVARCHAR(255) NULL,
        [Name]                          NVARCHAR(255) NULL,
        [Latitude]                      FLOAT NULL,
        [Longitude]                     FLOAT NULL,
        [Description]                   NVARCHAR(MAX) NULL,
        [Consumer]                      NVARCHAR(255) NULL,
        [DashboardId]                   UNIQUEIDENTIFIER NULL,
        [DashboardName]                 NVARCHAR(255) NULL,
        [CreatedOn]                     DATETIME2 NULL,
        [ObjectIdentifier]              NVARCHAR(100) NULL,
        [HasEvents]                     BIT NULL,
        [ActiveEvents]                  INT NULL,
        [ManagementOrganizationId]      UNIQUEIDENTIFIER NULL,
        [ManagementOrganizationName]    NVARCHAR(255) NULL,
        [Tags]                          NVARCHAR(MAX) NULL, -- JSON массив тегов
        [ImportedAt]                    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO