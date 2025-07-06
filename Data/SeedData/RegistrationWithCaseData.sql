-- 先確保有個案資料
INSERT INTO [NGOPlatformDb].[dbo].[Cases] 
([Name], [Phone], [IdentityNumber], [Birthday], [Address], [WorkerId], [Description], [CreatedAt], [Status], [Email], [Gender], [City], [District], [DetailAddress])
VALUES
('張小明', '0912345678', 'A123456789', '1990-01-15', '台北市信義區信義路五段7號', 1, '需要生活協助', GETDATE(), 'Active', 'zhang@email.com', 'Male', '台北市', '信義區', '信義路五段7號'),
('李小華', '0923456789', 'B234567890', '1985-03-20', '台北市大安區忠孝東路四段1號', 1, '需要就業輔導', GETDATE(), 'Active', 'li@email.com', 'Female', '台北市', '大安區', '忠孝東路四段1號'),
('王小美', '0934567890', 'C345678901', '1992-07-10', '新北市板橋區文化路一段100號', 2, '需要心理諮商', GETDATE(), 'Active', 'wang@email.com', 'Female', '新北市', '板橋區', '文化路一段100號'),
('陳大強', '0945678901', 'D456789012', '1988-11-25', '桃園市中壢區中正路200號', 2, '需要技能培訓', GETDATE(), 'Active', 'chen@email.com', 'Male', '桃園市', '中壢區', '中正路200號'),
('林小芳', '0956789012', 'E567890123', '1995-05-08', '台中市西區台灣大道一段1號', 3, '需要醫療協助', GETDATE(), 'Active', 'lin@email.com', 'Female', '台中市', '西區', '台灣大道一段1號'),
('黃志明', '0967890123', 'F678901234', '1983-09-12', '高雄市前金區中正路300號', 3, '需要住房協助', GETDATE(), 'Active', 'huang@email.com', 'Male', '高雄市', '前金區', '中正路300號'),
('劉雅婷', '0978901234', 'G789012345', '1991-12-03', '台南市東區東門路400號', 1, '需要教育協助', GETDATE(), 'Active', 'liu@email.com', 'Female', '台南市', '東區', '東門路400號'),
('吳建國', '0989012345', 'H890123456', '1987-04-18', '基隆市仁愛區仁一路500號', 2, '需要法律諮詢', GETDATE(), 'Active', 'wu@email.com', 'Male', '基隆市', '仁愛區', '仁一路500號'),
('周美玲', '0990123456', 'I901234567', '1993-08-22', '新竹市東區東門街600號', 3, '需要財務規劃', GETDATE(), 'Active', 'zhou@email.com', 'Female', '新竹市', '東區', '東門街600號'),
('鄭文雄', '0901234567', 'J012345678', '1986-02-14', '嘉義市西區中山路700號', 1, '需要家庭輔導', GETDATE(), 'Active', 'zheng@email.com', 'Male', '嘉義市', '西區', '中山路700號'),
('謝淑芬', '0912345679', 'K123456789', '1994-06-30', '宜蘭縣宜蘭市中山路800號', 2, '需要長照服務', GETDATE(), 'Active', 'xie@email.com', 'Female', '宜蘭縣', '宜蘭市', '中山路800號'),
('楊志豪', '0923456780', 'L234567890', '1989-10-05', '花蓮縣花蓮市中山路900號', 3, '需要身心障礙協助', GETDATE(), 'Active', 'yang@email.com', 'Male', '花蓮縣', '花蓮市', '中山路900號'),
('許雅文', '0934567891', 'M345678901', '1996-01-28', '台東縣台東市中山路1000號', 1, '需要原住民文化協助', GETDATE(), 'Active', 'xu@email.com', 'Female', '台東縣', '台東市', '中山路1000號'),
('蔡明德', '0945678902', 'N456789012', '1984-07-16', '澎湖縣馬公市中正路1100號', 2, '需要離島醫療協助', GETDATE(), 'Active', 'cai@email.com', 'Male', '澎湖縣', '馬公市', '中正路1100號'),
('郭麗華', '0956789013', 'O567890123', '1990-12-09', '金門縣金城鎮民生路1200號', 3, '需要新住民協助', GETDATE(), 'Active', 'guo@email.com', 'Female', '金門縣', '金城鎮', '民生路1200號');

-- 報名資料表假資料
-- 個案報名資料 (CaseActivityRegistrations)
INSERT INTO [NGOPlatformDb].[dbo].[CaseActivityRegistrations] 
([CaseId], [ActivityId], [Status])
VALUES
(1, 1, 'Pending'),
(2, 1, 'Approved'),
(3, 2, 'Approved'),
(4, 2, 'Cancelled'),
(5, 3, 'Pending'),
(6, 3, 'Approved'),
(7, 4, 'Pending'),
(8, 4, 'Approved'),
(9, 5, 'Cancelled'),
(10, 5, 'Pending'),
(11, 6, 'Approved'),
(12, 6, 'Pending'),
(13, 7, 'Approved'),
(14, 7, 'Cancelled'),
(15, 8, 'Pending');

-- 一般使用者報名資料 (UserActivityRegistrations)
INSERT INTO [NGOPlatformDb].[dbo].[UserActivityRegistrations] 
([UserId], [ActivityId], [Status], [NumberOfCompanions])
VALUES
(101, 1, 'Pending', 0),
(102, 1, 'Approved', 2),
(103, 2, 'Approved', 1),
(104, 2, 'Cancelled', 0),
(105, 3, 'Pending', 3),
(106, 3, 'Approved', 0),
(107, 4, 'Pending', 1),
(108, 4, 'Approved', 2),
(109, 5, 'Cancelled', 0),
(110, 5, 'Pending', 1),
(111, 6, 'Approved', 0),
(112, 6, 'Pending', 2),
(113, 7, 'Approved', 1),
(114, 7, 'Cancelled', 0),
(115, 8, 'Pending', 3),
(116, 9, 'Approved', 0),
(117, 9, 'Pending', 1),
(118, 10, 'Approved', 2),
(119, 10, 'Cancelled', 0),
(120, 11, 'Pending', 1);

-- 查詢結果驗證
SELECT 'Cases' as TableName, COUNT(*) as RecordCount 
FROM [NGOPlatformDb].[dbo].[Cases]
UNION ALL
SELECT 'CaseActivityRegistrations' as TableName, COUNT(*) as RecordCount 
FROM [NGOPlatformDb].[dbo].[CaseActivityRegistrations]
UNION ALL
SELECT 'UserActivityRegistrations' as TableName, COUNT(*) as RecordCount 
FROM [NGOPlatformDb].[dbo].[UserActivityRegistrations];

-- 測試查詢個案報名資料（包含個案姓名和活動名稱）
SELECT 
    car.Id,
    c.Name as CaseName,
    a.ActivityName,
    car.Status
FROM [NGOPlatformDb].[dbo].[CaseActivityRegistrations] car
JOIN [NGOPlatformDb].[dbo].[Cases] c ON car.CaseId = c.CaseId
JOIN [NGOPlatformDb].[dbo].[Activities] a ON car.ActivityId = a.ActivityId; 