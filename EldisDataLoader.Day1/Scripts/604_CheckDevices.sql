USE  EldisData
GO


-- ==========================================
-- Проверка таблицы EldisDevices
-- ==========================================

-- 1. Общая статистика по приборам
SELECT 
    COUNT(*) AS [Всего приборов в БД],
    COUNT(DISTINCT [DeviceModel]) AS [Уникальных моделей приборов],
    SUM(CASE WHEN [ModemIsBusy] = 1 THEN 1 ELSE 0 END) AS [Приборов с занятым модемом]
FROM [dbo].[EldisDevices];

-- 2. Просмотр последних 20 добавленных приборов
SELECT --TOP 20
    [LINK],
    [DeviceName] AS [Имя прибора],
    [DeviceModel] AS [Модель],
    [AddressObject] AS [Адрес объекта],
    [LastConnection] AS [Последнее подключение],
    [ImportedAt] AS [Дата импорта]
FROM [dbo].[EldisDevices]
ORDER BY [LINK] DESC;