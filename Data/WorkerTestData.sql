-- NGO 個案管理系統 - 工作人員測試資料
-- 用於 Workers 表的假資料

-- 清空現有資料（可選）
-- DELETE FROM Workers;

-- 插入工作人員測試資料
INSERT INTO Workers (Email, Password, Name) VALUES
-- 工作人員 1：系統管理員
('admin@ngo.org', 'admin123', '系統管理員'),

-- 工作人員 2：社工師
('social.worker@ngo.org', 'password123', '王社工'),

-- 工作人員 3：個案管理員
('case.manager@ngo.org', 'case123', '李個管'),

-- 工作人員 4：活動專員
('activity.staff@ngo.org', 'activity123', '陳活動'),

-- 工作人員 5：督導
('supervisor@ngo.org', 'super123', '張督導');

-- 查詢插入的資料
SELECT 
    WorkerId,
    Email,
    Password,
    Name
FROM Workers
ORDER BY WorkerId;

-- 統計查詢
SELECT 
    '總工作人員數' as 統計項目,
    COUNT(*) as 數量
FROM Workers; 