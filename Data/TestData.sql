-- NGO 個案管理系統 - 測試資料
-- 用於 Cases 表的假資料

-- 清空現有資料（可選）
-- DELETE FROM Cases;

-- 插入個案假資料
INSERT INTO Cases (Name, Phone, IdentityNumber, Birthday, Address, WorkerId, Description, CreatedAt, Status, Email, Gender, ProfileImage, City, District, DetailAddress) VALUES
-- 個案 1：經濟困難
('張小明', '0912345678', 'A123456789', '1995-03-15', '台北市中正區忠孝東路一段1號', 1, '經濟困難', '2024-01-15 09:30:00', 'Active', 'zhang.xiaoming@email.com', 'Male', NULL, '台北市', '中正區', '忠孝東路一段1號'),

-- 個案 2：家庭問題
('李小花', '0923456789', 'B234567890', '1988-07-22', '新北市板橋區文化路二段100號', 1, '家庭問題', '2024-01-16 14:20:00', 'Active', 'li.xiaohua@email.com', 'Female', NULL, '新北市', '板橋區', '文化路二段100號'),

-- 個案 3：學習困難
('陳小美', '0934567890', 'C345678901', '2002-11-08', '台中市西屯區河南路三段200號', 1, '學習困難', '2024-01-17 10:15:00', 'Active', 'chen.xiaomei@email.com', 'Female', NULL, '台中市', '西屯區', '河南路三段200號'),

-- 個案 4：健康問題
('王大華', '0945678901', 'D456789012', '1975-12-03', '高雄市前金區中正路四段300號', 1, '健康問題', '2024-01-18 16:45:00', 'Active', 'wang.dahua@email.com', 'Male', NULL, '高雄市', '前金區', '中正路四段300號'),

-- 個案 5：行為問題
('林小強', '0956789012', 'E567890123', '2000-05-18', '桃園市中壢區中央西路五段400號', 1, '行為問題', '2024-01-19 11:30:00', 'Active', 'lin.xiaoqiang@email.com', 'Male', NULL, '桃園市', '中壢區', '中央西路五段400號'),

-- 個案 6：人際關係
('黃小芳', '0967890123', 'F678901234', '1992-09-25', '台南市東區中華東路六段500號', 1, '人際關係', '2024-01-20 13:20:00', 'Active', 'huang.xiaofang@email.com', 'Female', NULL, '台南市', '東區', '中華東路六段500號'),

-- 個案 7：情緒困擾
('劉小偉', '0978901234', 'G789012345', '1998-01-12', '基隆市信義區信一路七段600號', 1, '情緒困擾', '2024-01-21 15:10:00', 'Active', 'liu.xiaowei@email.com', 'Male', NULL, '基隆市', '信義區', '信一路七段600號'),

-- 個案 8：其他困難
('周小玲', '0989012345', 'H890123456', '1985-06-30', '新竹市東區光復路八段700號', 1, '其他困難', '2024-01-22 09:45:00', 'Active', 'zhou.xiaoling@email.com', 'Female', NULL, '新竹市', '東區', '光復路八段700號'),

-- 個案 9：經濟困難（已結案）
('吳小傑', '0990123456', 'I901234567', '1990-04-14', '嘉義市西區中山路九段800號', 1, '經濟困難', '2024-01-10 12:00:00', 'Completed', 'wu.xiaojie@email.com', 'Male', NULL, '嘉義市', '西區', '中山路九段800號'),

-- 個案 10：家庭問題（暫停）
('鄭小雅', '0991234567', 'J012345678', '1993-08-07', '宜蘭縣宜蘭市中山路十段900號', 1, '家庭問題', '2024-01-12 14:30:00', 'Suspended', 'zheng.xiaoya@email.com', 'Female', NULL, '宜蘭縣', '宜蘭市', '中山路十段900號'),

-- 個案 11：學習困難
('謝小龍', '0992345678', 'K123456789', '2005-02-28', '苗栗縣苗栗市中正路十一段1000號', 1, '學習困難', '2024-01-23 10:20:00', 'Active', 'xie.xiaolong@email.com', 'Male', NULL, '苗栗縣', '苗栗市', '中正路十一段1000號'),

-- 個案 12：健康問題
('許小婷', '0993456789', 'L234567890', '1987-10-16', '彰化縣彰化市中華路十二段1100號', 1, '健康問題', '2024-01-24 16:15:00', 'Active', 'xu.xiaoting@email.com', 'Female', NULL, '彰化縣', '彰化市', '中華路十二段1100號'),

-- 個案 13：行為問題
('郭小豪', '0994567890', 'M345678901', '1999-12-05', '南投縣南投市中興路十三段1200號', 1, '行為問題', '2024-01-25 11:40:00', 'Active', 'guo.xiaohao@email.com', 'Male', NULL, '南投縣', '南投市', '中興路十三段1200號'),

-- 個案 14：人際關係
('何小雯', '0995678901', 'N456789012', '1991-03-21', '雲林縣斗六市中山路十四段1300號', 1, '人際關係', '2024-01-26 13:50:00', 'Active', 'he.xiaowen@email.com', 'Female', NULL, '雲林縣', '斗六市', '中山路十四段1300號'),

-- 個案 15：情緒困擾
('高小志', '0996789012', 'O567890123', '1996-07-11', '屏東縣屏東市自由路十五段1400號', 1, '情緒困擾', '2024-01-27 15:25:00', 'Active', 'gao.xiaozhi@email.com', 'Male', NULL, '屏東縣', '屏東市', '自由路十五段1400號');

-- 查詢插入的資料
SELECT 
    CaseId,
    Name,
    Phone,
    IdentityNumber,
    Birthday,
    Address,
    WorkerId,
    Description,
    CreatedAt,
    Status,
    Email,
    Gender,
    ProfileImage,
    City,
    District,
    DetailAddress
FROM Cases
ORDER BY CreatedAt DESC;

-- 統計查詢
SELECT 
    '總個案數' as 統計項目,
    COUNT(*) as 數量
FROM Cases
UNION ALL
SELECT 
    '活躍個案' as 統計項目,
    COUNT(*) as 數量
FROM Cases
WHERE Status = 'Active'
UNION ALL
SELECT 
    '已結案' as 統計項目,
    COUNT(*) as 數量
FROM Cases
WHERE Status = 'Completed'
UNION ALL
SELECT 
    '暫停個案' as 統計項目,
    COUNT(*) as 數量
FROM Cases
WHERE Status = 'Suspended';

-- 按困難類型統計
SELECT 
    Description as 困難類型,
    COUNT(*) as 個案數量
FROM Cases
GROUP BY Description
ORDER BY COUNT(*) DESC; 