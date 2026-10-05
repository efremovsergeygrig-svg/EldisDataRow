USE EldisData
GO

-- Общая статистика по точкам учёта
SELECT 
    'MeteringPoints CNT',
    COUNT(*) AS [Всего точек],
    COUNT(DISTINCT [ResourceId]) AS [Уникальных ресурсов],
    SUM(CASE WHEN [IsHeat] = 1 THEN 1 ELSE 0 END) AS [Точек отопления],
    SUM(CASE WHEN [IsGvs] = 1 THEN 1 ELSE 0 END) AS [Точек ГВС]
FROM [dbo].[EldisMeteringPoints];

-- Последние 10 импортированных точек
IF 1 = 1
    SELECT -- TOP 10
        'MeteringPoints',
        [LINK],
        [SerialNumber],
        [DeviceName],
        [Address],
        [ImportedAt]
    FROM [dbo].[EldisMeteringPoints]
    ORDER BY [LINK] DESC;