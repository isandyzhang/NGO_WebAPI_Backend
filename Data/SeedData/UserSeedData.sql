-- 使用者假資料
INSERT INTO [NGOPlatformDb].[dbo].[Users] 
([UserId], [IdentityNumber], [Email], [Password], [Phone], [Name])
VALUES
(101, 'A101234567', 'user101@email.com', 'password123', '0912345678', '陳志明'),
(102, 'B102345678', 'user102@email.com', 'password123', '0923456789', '林美玲'),
(103, 'C103456789', 'user103@email.com', 'password123', '0934567890', '王建國'),
(104, 'D104567890', 'user104@email.com', 'password123', '0945678901', '張雅婷'),
(105, 'E105678901', 'user105@email.com', 'password123', '0956789012', '李志豪'),
(106, 'F106789012', 'user106@email.com', 'password123', '0967890123', '劉淑芬'),
(107, 'G107890123', 'user107@email.com', 'password123', '0978901234', '吳文雄'),
(108, 'H108901234', 'user108@email.com', 'password123', '0989012345', '黃雅文'),
(109, 'I109012345', 'user109@email.com', 'password123', '0990123456', '周麗華'),
(110, 'J110123456', 'user110@email.com', 'password123', '0901234567', '鄭明德'),
(111, 'K111234567', 'user111@email.com', 'password123', '0912345679', '謝淑芬'),
(112, 'L112345678', 'user112@email.com', 'password123', '0923456780', '楊志豪'),
(113, 'M113456789', 'user113@email.com', 'password123', '0934567891', '許雅文'),
(114, 'N114567890', 'user114@email.com', 'password123', '0945678902', '蔡明德'),
(115, 'O115678901', 'user115@email.com', 'password123', '0956789013', '郭麗華'),
(116, 'P116789012', 'user116@email.com', 'password123', '0967890124', '徐志明'),
(117, 'Q117890123', 'user117@email.com', 'password123', '0978901235', '孫美玲'),
(118, 'R118901234', 'user118@email.com', 'password123', '0989012346', '朱建國'),
(119, 'S119012345', 'user119@email.com', 'password123', '0990123457', '胡雅婷'),
(120, 'T120123456', 'user120@email.com', 'password123', '0901234568', '馬志豪');

-- 查詢結果驗證
SELECT 'Users' as TableName, COUNT(*) as RecordCount 
FROM [NGOPlatformDb].[dbo].[Users];

-- 測試關聯查詢
SELECT TOP 5
    uar.Id,
    uar.UserId,
    u.Name as UserName,
    uar.ActivityId,
    a.ActivityName,
    uar.Status,
    uar.NumberOfCompanions
FROM [NGOPlatformDb].[dbo].[UserActivityRegistrations] uar
LEFT JOIN [NGOPlatformDb].[dbo].[Users] u ON uar.UserId = u.UserId
LEFT JOIN [NGOPlatformDb].[dbo].[Activities] a ON uar.ActivityId = a.ActivityId; 