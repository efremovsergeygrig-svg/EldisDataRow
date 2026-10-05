-- Создание пользователей
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EldisUsers')
BEGIN
    CREATE TABLE [dbo].[EldisUsers]
    (
        [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DomainLogin]   NVARCHAR(100) NOT NULL UNIQUE,  -- Например: 'INTERRAO\username' или 'username'
        [FullName]      NVARCHAR(200) NULL,             -- Для отображения: 'Иванов Иван Иванович'
        [Role]          NVARCHAR(50) NOT NULL DEFAULT 'User', -- Administrator, User, Viewer
        [IsActive]      BIT NOT NULL DEFAULT 1,         -- 1 = активен, 0 = заблокирован
        [CreatedAt]     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [LastLoginAt]   DATETIME2 NULL                  -- Когда последний раз заходил
    );

    -- Добавляем тестовых пользователей (замените на реальные доменные имена!)
    INSERT INTO [dbo].[EldisUsers] ([DomainLogin], [FullName], [Role])
    VALUES 
        ('INTERRAO\efremov_sg', 'Ефремов Сергей Григорьевич', 'Administrator'),  -- <-- ЗАМЕНИТЕ на ваш логин!
        ('INTERRAO\grokhotov_vy', 'Грохотов Валерий  Юрьевич', 'User'),
        ('INTERRAO\basmanov_av', 'Басманов Александр Владимирович', 'Viewer');
    END
GO




-- Создание таблицы точек учёта
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EldisMeteringPoints')
BEGIN
    CREATE TABLE [dbo].[EldisMeteringPoints]
    (
        [LINK]                   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Id]                     UNIQUEIDENTIFIER NOT NULL UNIQUE,
        [DeviceId]               UNIQUEIDENTIFIER NULL,
        [ObjectId]               UNIQUEIDENTIFIER NULL,
        [Status]                 INT NULL,
        [Identifier]             NVARCHAR(100) NULL,
        [Identifier2]            NVARCHAR(100) NULL,
        [Address]                NVARCHAR(500) NULL,
        [DeviceName]             NVARCHAR(255) NULL,
        [DeviceCode]             INT NULL,
        [ModelModificationId]    UNIQUEIDENTIFIER NULL,
        [ModelModificationName]  NVARCHAR(255) NULL,
        [CustomModelName]        NVARCHAR(255) NULL,
        [SerialNumber]           NVARCHAR(100) NULL,
        [ResourceId]             UNIQUEIDENTIFIER NULL,
        [ResourceCode]           INT NULL,
        [ResourceName]           NVARCHAR(100) NULL,
        [MeasurePointNumber]     NVARCHAR(100) NULL,
        [MeasurePointName]       NVARCHAR(255) NULL,
        [IsHeat]                 BIT NULL,
        [IsGvs]                  BIT NULL,
        [SystemHeat]             INT NULL,
        [SystemGvs]              INT NULL,
        [SchemeGvs]              INT NULL,
        [InputConfiguration]     INT NULL,
        [CreatedOn]              DATETIME2 NULL,
        [Description]            NVARCHAR(MAX) NULL,
        [IsReadOnly]             BIT NOT NULL DEFAULT 0,
        [IsArchivalRecord]       BIT NOT NULL DEFAULT 0,
        [MeteoStationId]         UNIQUEIDENTIFIER NULL,
        [ModemSn]                NVARCHAR(100) NULL,
        [ModemModelName]         NVARCHAR(255) NULL,
        [ModemModelCode]         INT NULL,
        [IsAccounting]           BIT NULL,
        [ImportedAt]             DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- Создание таблицы сырых данных
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EldisRawData')
BEGIN
    CREATE TABLE [dbo].[EldisRawData]
    (
        [LINK]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MeteringPointId]   UNIQUEIDENTIFIER NOT NULL,
        [DtMeasure]         DATETIME2 NOT NULL,
        [M_Itog1_TR]        FLOAT NULL,
        [M_Itog2_TR]        FLOAT NULL,
        [Mg_Itog_TV]        FLOAT NULL,
        [Qg_Itog_TV]        FLOAT NULL,
        [V_Itog1_TR]        FLOAT NULL,
        [V_Itog2_TR]        FLOAT NULL,
        [t1_TR]             FLOAT NULL,
        [t2_TR]             FLOAT NULL,
        [V1_TR]             FLOAT NULL,
        [V2_TR]             FLOAT NULL,
        [M1_TR]             FLOAT NULL,
        [M2_TR]             FLOAT NULL,
        [P1_TR]             FLOAT NULL,
        [P2_TR]             FLOAT NULL,
        [Mg_TV]             FLOAT NULL,
        [Qo_TV]             FLOAT NULL,
        [Qg_TV]             FLOAT NULL,
        [Qo_Itog_TV]        FLOAT NULL,
        [dt_TV]             FLOAT NULL,
        [tsw_TV]            FLOAT NULL,
        [ta_TV]             FLOAT NULL,
        [QntHIP_TV]         FLOAT NULL,
        [QntP_TV]           FLOAT NULL,
        [QntHIP_Itog_TV]    FLOAT NULL,
        [NS_TV]             BIT NULL,
        [DI_TV]             FLOAT NULL,
        [TypeDataCode]      INT NOT NULL,
        [StartDate]         DATETIME2 NOT NULL,
        [EndDate]           DATETIME2 NOT NULL,
        [ImportedAt]        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        CONSTRAINT [FK_EldisRawData_MeteringPoints] FOREIGN KEY ([MeteringPointId]) 
            REFERENCES [dbo].[EldisMeteringPoints]([Id])
    );
END
GO