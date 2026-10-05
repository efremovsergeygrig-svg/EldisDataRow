USE  EldisData
GO



-- ==========================================
-- Связная проверка: Приборы + Их Объекты
-- ==========================================

SELECT --TOP 20
    d.[DeviceName] AS [Прибор учёта],
    d.[DeviceModel] AS [Модель прибора],
    d.[AddressObject] AS [Адрес из таблицы приборов],
    o.[Name] AS [Название объекта из таблицы объектов],
    o.[Address] AS [Адрес из таблицы объектов (маскированный)]
FROM [dbo].[EldisDevices] d
LEFT JOIN [dbo].[EldisObjects] o ON d.[ObjectId] = o.[Id]
ORDER BY d.[LINK] DESC;