-- ==========================================
-- Список всех пользовательских таблиц в базе
-- (поимённо, с количеством строк и размером)
-- ==========================================

SELECT 
    ROW_NUMBER() OVER (ORDER BY t.[name]) AS [№],
    s.[name] AS [Схема],
    t.[name] AS [Имя таблицы],
    p.[rows] AS [Количество строк],
    CAST(ROUND(SUM(a.total_pages) * 8.0 / 1024, 2) AS DECIMAL(10,2)) AS [Размер МБ],
    t.[create_date] AS [Дата создания],
    t.[modify_date] AS [Дата изменения]
FROM sys.tables t
INNER JOIN sys.schemas s ON t.[schema_id] = s.[schema_id]
INNER JOIN sys.indexes i ON t.[object_id] = i.[object_id]
INNER JOIN sys.partitions p ON i.[object_id] = p.[object_id] AND i.[index_id] = p.[index_id]
INNER JOIN sys.allocation_units a ON p.[partition_id] = a.container_id
WHERE t.[is_ms_shipped] = 0
  AND i.[index_id] IN (0, 1)
GROUP BY 
    s.[name], 
    t.[name], 
    p.[rows], 
    t.[create_date], 
    t.[modify_date]
ORDER BY t.[name];


USE EldisData;
GO

-- ==========================================
-- Список всех пользовательских таблиц в базе
-- ==========================================

SELECT 
    ROW_NUMBER() OVER (ORDER BY t.[name]) AS [№],
    s.[name] AS [Схема],
    t.[name] AS [Имя таблицы],
    p.[rows] AS [Количество строк],
    CAST(ROUND(SUM(a.total_pages) * 8.0 / 1024, 2) AS DECIMAL(10,2)) AS [Размер МБ],
    t.[create_date] AS [Дата создания],
    t.[modify_date] AS [Дата изменения]
FROM sys.tables t
INNER JOIN sys.schemas s ON t.[schema_id] = s.[schema_id]
INNER JOIN sys.indexes i ON t.[object_id] = i.[object_id]
INNER JOIN sys.partitions p ON i.[object_id] = p.[object_id] AND i.[index_id] = p.[index_id]
INNER JOIN sys.allocation_units a ON p.[partition_id] = a.container_id
WHERE t.[is_ms_shipped] = 0
  AND i.[index_id] IN (0, 1)
GROUP BY 
    s.[name], 
    t.[name], 
    p.[rows], 
    t.[create_date], 
    t.[modify_date]
ORDER BY t.[name];



SELECT  
        *
    FROM
        [dbo].[EldisUsers]

SELECT SYSTEM_USER;  -- Покажет DOMAIN\Username
SELECT ORIGINAL_LOGIN();  -- Альтернативный вариант
        