-- NGO 個案管理系統 - 報名審核測試資料
-- 用於 CaseActivityRegistrations 和 UserActivityRegistrations 表的假資料

-- 清空現有資料（可選）
-- DELETE FROM CaseActivityRegistrations;
-- DELETE FROM UserActivityRegistrations;

-- 插入個案活動報名測試資料
INSERT INTO CaseActivityRegistrations (CaseId, ActivityId, Status) VALUES
-- 個案 1 報名活動 1（待審核）
(1, 1, 'Pending'),
-- 個案 2 報名活動 1（已批准）
(2, 1, 'Approved'),
-- 個案 3 報名活動 2（待審核）
(3, 2, 'Pending'),
-- 個案 4 報名活動 2（已拒絕）
(4, 2, 'Cancelled'),
-- 個案 5 報名活動 3（待審核）
(5, 3, 'Pending'),
-- 個案 6 報名活動 3（已批准）
(6, 3, 'Approved'),
-- 個案 7 報名活動 4（待審核）
(7, 4, 'Pending'),
-- 個案 8 報名活動 1（待審核）
(8, 1, 'Pending'),
-- 個案 9 報名活動 2（已批准）
(9, 2, 'Approved'),
-- 個案 10 報名活動 3（待審核）
(10, 3, 'Pending'),
-- 個案 11 報名活動 4（已批准）
(11, 4, 'Approved'),
-- 個案 12 報名活動 1（待審核）
(12, 1, 'Pending'),
-- 個案 1 報名活動 2（已批准）
(1, 2, 'Approved'),
-- 個案 2 報名活動 3（待審核）
(2, 3, 'Pending'),
-- 個案 3 報名活動 4（已拒絕）
(3, 4, 'Cancelled');

-- 插入民眾活動報名測試資料
INSERT INTO UserActivityRegistrations (UserId, ActivityId, Status, NumberOfCompanions) VALUES
-- 民眾 1 報名活動 1（待審核，帶 2 人）
(1, 1, 'Pending', 2),
-- 民眾 2 報名活動 1（已批准，帶 1 人）
(2, 1, 'Approved', 1),
-- 民眾 3 報名活動 2（待審核，不帶人）
(3, 2, 'Pending', 0),
-- 民眾 4 報名活動 2（已拒絕，帶 3 人）
(4, 2, 'Cancelled', 3),
-- 民眾 5 報名活動 3（待審核，帶 1 人）
(5, 3, 'Pending', 1),
-- 民眾 1 報名活動 3（已批准，不帶人）
(1, 3, 'Approved', 0),
-- 民眾 2 報名活動 4（待審核，帶 2 人）
(2, 4, 'Pending', 2),
-- 民眾 3 報名活動 4（已批准，帶 1 人）
(3, 4, 'Approved', 1),
-- 民眾 4 報名活動 1（待審核，帶 4 人）
(4, 1, 'Pending', 4),
-- 民眾 5 報名活動 2（已批准，不帶人）
(5, 2, 'Approved', 0);

-- 查詢插入的個案報名資料
SELECT 
    car.Id,
    car.CaseId,
    c.Name as CaseName,
    car.ActivityId,
    a.ActivityName,
    car.Status
FROM CaseActivityRegistrations car
LEFT JOIN Cases c ON car.CaseId = c.CaseId
LEFT JOIN Activities a ON car.ActivityId = a.ActivityId
ORDER BY car.Id;

-- 查詢插入的民眾報名資料
SELECT 
    uar.Id,
    uar.UserId,
    u.Name as UserName,
    uar.ActivityId,
    a.ActivityName,
    uar.NumberOfCompanions,
    uar.Status
FROM UserActivityRegistrations uar
LEFT JOIN Users u ON uar.UserId = u.UserId
LEFT JOIN Activities a ON uar.ActivityId = a.ActivityId
ORDER BY uar.Id;

-- 統計查詢
SELECT 
    '個案報名總數' as 統計項目,
    COUNT(*) as 數量
FROM CaseActivityRegistrations
UNION ALL
SELECT 
    '個案待審核' as 統計項目,
    COUNT(*) as 數量
FROM CaseActivityRegistrations
WHERE Status = 'Pending'
UNION ALL
SELECT 
    '民眾報名總數' as 統計項目,
    COUNT(*) as 數量
FROM UserActivityRegistrations
UNION ALL
SELECT 
    '民眾待審核' as 統計項目,
    COUNT(*) as 數量
FROM UserActivityRegistrations
WHERE Status = 'Pending'; 