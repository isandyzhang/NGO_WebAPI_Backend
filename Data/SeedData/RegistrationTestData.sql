-- 報名審核測試資料
-- 請先確保 Cases, Activities, Users 表中有資料

-- 插入個案活動報名測試資料
INSERT INTO CaseActivityRegistrations (CaseId, ActivityId, Status)
VALUES 
  (1, 1, 'Pending'),
  (2, 1, 'Approved'),
  (3, 2, 'Pending'),
  (1, 2, 'Cancelled'),
  (2, 3, 'Pending');

-- 插入使用者活動報名測試資料
INSERT INTO UserActivityRegistrations (UserId, ActivityId, Status, NumberOfCompanions)
VALUES 
  (1, 1, 'Pending', 2),
  (2, 1, 'Approved', 0),
  (3, 2, 'Pending', 1),
  (1, 2, 'Cancelled', 3),
  (2, 3, 'Pending', 0);

-- 查詢個案報名資料 (用於測試)
SELECT 
  r.Id,
  c.Name as CaseName,
  a.ActivityName,
  r.Status
FROM CaseActivityRegistrations r
JOIN Cases c ON r.CaseId = c.Id
JOIN Activities a ON r.ActivityId = a.ActivityId;

-- 查詢使用者報名資料 (用於測試)
SELECT 
  r.Id,
  r.UserId,
  u.Name as UserName,
  a.ActivityName,
  r.NumberOfCompanions,
  r.Status
FROM UserActivityRegistrations r
JOIN Users u ON r.UserId = u.Id
JOIN Activities a ON r.ActivityId = a.ActivityId; 