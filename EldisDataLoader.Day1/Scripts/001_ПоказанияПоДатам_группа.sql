SELECT 
		 CAST([DtMeasure] AS DATE)		[DtMeasure]
		,COUNT(*) AS [Количество записей]
FROM [dbo].[EldisRawData]
--WHERE CAST([DtMeasure] AS DATE) = CAST(GETDATE() - 1 AS DATE)
GROUP BY
		 CAST([DtMeasure] AS DATE)
;