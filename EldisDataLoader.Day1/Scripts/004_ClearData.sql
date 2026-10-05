--04_ClearData.sql
USE EldisData
GO

-- ==========================================
-- ОЧИСТКА ТАБЛИЦ БАЗЫ ДАННЫХ
-- ⚠️ ВНИМАНИЕ: операция необратима!
-- 
-- Переключатели:
--   1 = очистить таблицу
--   0 = пропустить таблицу
-- ==========================================

SET NOCOUNT ON;

-- Настройки очистки
DECLARE @ClearObjects      BIT = 1;  -- EldisObjects
DECLARE @ClearDevices      BIT = 1;  -- EldisDevices
DECLARE @ClearPoints       BIT = 1;  -- EldisMeteringPoints
DECLARE @ClearRawData      BIT = 0;  -- EldisRawData (по умолчанию НЕ очищаем!)

PRINT '=== НАСТРОЙКИ ОЧИСТКИ ===';
PRINT 'EldisObjects:       ' + CASE WHEN @ClearObjects = 1 THEN 'ОЧИСТИТЬ' ELSE 'ПРОПУСТИТЬ' END;
PRINT 'EldisDevices:       ' + CASE WHEN @ClearDevices = 1 THEN 'ОЧИСТИТЬ' ELSE 'ПРОПУСТИТЬ' END;
PRINT 'EldisMeteringPoints:' + CASE WHEN @ClearPoints  = 1 THEN 'ОЧИСТИТЬ' ELSE 'ПРОПУСТИТЬ' END;
PRINT 'EldisRawData:       ' + CASE WHEN @ClearRawData = 1 THEN 'ОЧИСТИТЬ' ELSE 'ПРОПУСТИТЬ' END;
PRINT '';

-- Статистика ДО очистки
PRINT '=== ДАННЫЕ ПЕРЕД ОЧИСТКОЙ ===';
SELECT 
    'EldisObjects'       AS [Таблица], COUNT(*) AS [Записей] FROM [dbo].[EldisObjects]
UNION ALL
SELECT 'EldisDevices',   COUNT(*) FROM [dbo].[EldisDevices]
UNION ALL
SELECT 'EldisMeteringPoints', COUNT(*) FROM [dbo].[EldisMeteringPoints]
UNION ALL
SELECT 'EldisRawData',   COUNT(*) FROM [dbo].[EldisRawData];
PRINT '';

-- ==========================================
-- 1. EldisRawData (дочерняя, с FK на точки учёта)
-- ==========================================
IF @ClearRawData = 1
BEGIN
    PRINT 'Очистка EldisRawData...';
    DELETE FROM [dbo].[EldisRawData];
    DBCC CHECKIDENT ('[dbo].[EldisRawData]', RESEED, 0);
    PRINT '✅ EldisRawData очищена.';
END
ELSE
BEGIN
    PRINT '⏭️  EldisRawData пропущена (история сохраняется).';
END

-- ==========================================
-- 2. EldisMeteringPoints (с FK на объекты и приборы)
-- ==========================================
IF @ClearPoints = 1
BEGIN
    PRINT 'Очистка EldisMeteringPoints...';
    DELETE FROM [dbo].[EldisMeteringPoints];
    DBCC CHECKIDENT ('[dbo].[EldisMeteringPoints]', RESEED, 0);
    PRINT '✅ EldisMeteringPoints очищена.';
END
ELSE
BEGIN
    PRINT '⏭️  EldisMeteringPoints пропущена.';
END

-- ==========================================
-- 3. EldisDevices (с FK на объекты)
-- ==========================================
IF @ClearDevices = 1
BEGIN
    PRINT 'Очистка EldisDevices...';
    DELETE FROM [dbo].[EldisDevices];
    DBCC CHECKIDENT ('[dbo].[EldisDevices]', RESEED, 0);
    PRINT '✅ EldisDevices очищена.';
END
ELSE
BEGIN
    PRINT '⏭️  EldisDevices пропущена.';
END

-- ==========================================
-- 4. EldisObjects (родительская таблица)
-- ==========================================
IF @ClearObjects = 1
BEGIN
    PRINT 'Очистка EldisObjects...';
    DELETE FROM [dbo].[EldisObjects];
    DBCC CHECKIDENT ('[dbo].[EldisObjects]', RESEED, 0);
    PRINT '✅ EldisObjects очищена.';
END
ELSE
BEGIN
    PRINT '⏭️  EldisObjects пропущена.';
END

-- Статистика ПОСЛЕ очистки
PRINT '';
PRINT '=== ДАННЫЕ ПОСЛЕ ОЧИСТКИ ===';
SELECT 
    'EldisObjects'       AS [Таблица], COUNT(*) AS [Записей] FROM [dbo].[EldisObjects]
UNION ALL
SELECT 'EldisDevices',   COUNT(*) FROM [dbo].[EldisDevices]
UNION ALL
SELECT 'EldisMeteringPoints', COUNT(*) FROM [dbo].[EldisMeteringPoints]
UNION ALL
SELECT 'EldisRawData',   COUNT(*) FROM [dbo].[EldisRawData];
PRINT '';
PRINT '=== ОЧИСТКА ЗАВЕРШЕНА ===';
GO