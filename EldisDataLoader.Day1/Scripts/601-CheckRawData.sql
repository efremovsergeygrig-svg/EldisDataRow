-- Scripts/ViewRawData.sql
USE  EldisData
GO

-- ==========================================
-- Просмотр сырых показаний (EldisRawData)
-- ==========================================

-- ==========================================
-- 1. ОБЩАЯ СТАТИСТИКА по таблице сырых данных
-- ==========================================
IF 1 = 10
SELECT 
    COUNT(*) AS [Всего записей],
    COUNT(DISTINCT [MeteringPointId]) AS [Уникальных точек учёта],
    MIN([DtMeasure]) AS [Самая ранняя дата],
    MAX([DtMeasure]) AS [Самая поздняя дата],
    MIN([ImportedAt]) AS [Первый импорт],
    MAX([ImportedAt]) AS [Последний импорт]
FROM [dbo].[EldisRawData];
GO

-- ==========================================
-- 1.1. СТАТИСТИКА ПО ДНЯМ (количество записей за каждый день)
-- ==========================================
IF 1 = 10
SELECT 
    CAST([DtMeasure] AS DATE) AS [Дата],
    COUNT(*) AS [Количество записей],
    COUNT(DISTINCT [MeteringPointId]) AS [Уникальных точек учёта],
    MIN([DtMeasure]) AS [Первое измерение дня],
    MAX([DtMeasure]) AS [Последнее измерение дня]
FROM [dbo].[EldisRawData]
GROUP BY CAST([DtMeasure] AS DATE)
ORDER BY [Дата] DESC;
GO

-- ==========================================
-- 2. РАСПРЕДЕЛЕНИЕ по типам данных
-- ==========================================
IF 1 = 10
SELECT 
    [TypeDataCode] AS [Код типа данных],
    COUNT(*) AS [Количество записей],
    COUNT(DISTINCT [MeteringPointId]) AS [Точек учёта]
FROM [dbo].[EldisRawData]
GROUP BY [TypeDataCode]
ORDER BY [Количество записей] DESC;
GO

-- ==========================================
-- 3. ПОСЛЕДНИЕ 50 ЗАПИСЕЙ с информацией о точке учёта
-- (самый информативный запрос)
-- ==========================================
IF 1 = 1
SELECT
    --TOP 50
    rd.[LINK] AS [№],
    mp.[SerialNumber] AS [№ ПУ],
    mp.[DeviceName] AS [Прибор учёта],
    mp.[Address] AS [Адрес],
    rd.[DtMeasure] AS [Дата измерения],
    rd.[TypeDataCode] AS [Тип данных],
    
    -- Тепловая энергия и масса
    rd.[Qg_Itog_TV] AS [Qg (Гкал)],
    rd.[Mg_Itog_TV] AS [Mg (т)],
    rd.[M_Itog1_TR] AS [M_Itog1 (т)],
    rd.[M_Itog2_TR] AS [M_Itog2 (т)],
    
    -- Температуры
    rd.[t1_TR] AS [t1 (°C)],
    rd.[t2_TR] AS [t2 (°C)],
    rd.[tsw_TV] AS [tsw (°C)],
    rd.[ta_TV] AS [ta (°C)],
    
    -- Давления
    rd.[P1_TR] AS [P1 (МПа)],
    rd.[P2_TR] AS [P2 (МПа)],
    
    -- Объёмы и расходы
    rd.[V_Itog1_TR] AS [V_Itog1],
    rd.[V_Itog2_TR] AS [V_Itog2],
    rd.[V1_TR] AS [V1],
    rd.[V2_TR] AS [V2],
    
    -- Массы по трубопроводам
    rd.[M1_TR] AS [M1 (т)],
    rd.[M2_TR] AS [M2 (т)],
    
    -- Параметры ТВ
    rd.[Qo_TV] AS [Qo_TV],
    rd.[Qg_TV] AS [Qg_TV],
    rd.[Qo_Itog_TV] AS [Qo_Itog_TV],
    rd.[dt_TV] AS [dt_TV],
    
    -- Наработка и диагностика
    rd.[QntHIP_TV] AS [QntHIP],
    rd.[QntP_TV] AS [QntP],
    rd.[QntHIP_Itog_TV] AS [QntHIP_Itog],
    rd.[NS_TV] AS [NS (флаг)],
    rd.[DI_TV] AS [DI],
    
    rd.[ImportedAt] AS [Дата импорта]
FROM [dbo].[EldisRawData] rd
LEFT JOIN [dbo].[EldisMeteringPoints] mp ON rd.[MeteringPointId] = mp.[Id]
ORDER BY rd.[DtMeasure] DESC, rd.[LINK] DESC;
GO

-- ==========================================
-- 4. ТОП-20 точек с наибольшим количеством записей
-- ==========================================
IF 1 = 10
SELECT 
    --TOP 20
    mp.[SerialNumber] AS [Серийный номер ПУ],
    mp.[DeviceName] AS [Прибор учёта],
    COUNT(rd.[LINK]) AS [Количество записей],
    MIN(rd.[DtMeasure]) AS [Первая дата],
    MAX(rd.[DtMeasure]) AS [Последняя дата]
FROM [dbo].[EldisRawData] rd
INNER JOIN [dbo].[EldisMeteringPoints] mp ON rd.[MeteringPointId] = mp.[Id]
GROUP BY mp.[SerialNumber], mp.[DeviceName]
ORDER BY [Количество записей] DESC;
GO

-- ==========================================
-- 5. ПОИСК ПО СЕР