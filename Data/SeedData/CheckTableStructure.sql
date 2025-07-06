-- 檢查資料表結構
-- 檢查 CaseActivityRegistrations 表結構
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'CaseActivityRegistrations'
ORDER BY ORDINAL_POSITION;

-- 檢查 UserActivityRegistrations 表結構
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserActivityRegistrations'
ORDER BY ORDINAL_POSITION;

-- 檢查 Cases 表結構
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Cases'
ORDER BY ORDINAL_POSITION;

-- 檢查 Activities 表結構
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Activities'
ORDER BY ORDINAL_POSITION;

-- 檢查現有資料
SELECT 'CaseActivityRegistrations' as TableName, COUNT(*) as RecordCount 
FROM [NGOPlatformDb].[dbo].[CaseActivityRegistrations]
UNION ALL
SELECT 'Cases' as TableName, COUNT(*) as RecordCount 
FROM [NGOPlatformDb].[dbo].[Cases]
UNION ALL
SELECT 'Activities' as TableName, COUNT(*) as RecordCount 
FROM [NGOPlatformDb].[dbo].[Activities];

-- 測試關聯查詢
SELECT TOP 5
    car.Id,
    car.CaseId,
    car.ActivityId,
    c.Name as CaseName,
    a.ActivityName,
    car.Status
FROM [NGOPlatformDb].[dbo].[CaseActivityRegistrations] car
LEFT JOIN [NGOPlatformDb].[dbo].[Cases] c ON car.CaseId = c.CaseId
LEFT JOIN [NGOPlatformDb].[dbo].[Activities] a ON car.ActivityId = a.ActivityId; 