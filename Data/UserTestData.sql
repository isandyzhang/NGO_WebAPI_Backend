-- NGO 個案管理系統 - 用戶測試資料
-- 用於 Users 表的假資料

-- 清空現有資料（可選）
-- DELETE FROM Users;

-- 插入用戶測試資料
INSERT INTO Users (IdentityNumber, Email, Password, Phone, Name) VALUES
-- 用戶 1
('A100000001', 'user1@example.com', 'password123', '0911111111', '王小明'),

-- 用戶 2
('A200000002', 'user2@example.com', 'password123', '0922222222', '李小華'),

-- 用戶 3
('A300000003', 'user3@example.com', 'password123', '0933333333', '張小美'),

-- 用戶 4
('A400000004', 'user4@example.com', 'password123', '0944444444', '陳小強'),

-- 用戶 5
('A500000005', 'user5@example.com', 'password123', '0955555555', '林小芳'),

-- 用戶 6
('A600000006', 'user6@example.com', 'password123', '0966666666', '黃小偉'),

-- 用戶 7
('A700000007', 'user7@example.com', 'password123', '0977777777', '吳小玲'),

-- 用戶 8
('A800000008', 'user8@example.com', 'password123', '0988888888', '劉小傑'),

-- 用戶 9
('A900000009', 'user9@example.com', 'password123', '0999999999', '周小雅'),

-- 用戶 10
('B100000010', 'user10@example.com', 'password123', '0900000000', '鄭小龍');

-- 查詢插入的資料
SELECT 
    UserId,
    IdentityNumber,
    Email,
    Phone,
    Name
FROM Users
ORDER BY UserId;

-- 統計查詢
SELECT 
    '總用戶數' as 統計項目,
    COUNT(*) as 數量
FROM Users; 