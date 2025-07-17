-- 測試用戶權限系統的工作人員數據
-- 請在開發環境中執行此腳本

-- 首先檢查是否已存在測試用戶
IF NOT EXISTS (SELECT 1 FROM Workers WHERE Email = 'staff.a@ngo.org')
BEGIN
    -- 員工A - 負責案例 1, 2, 3
    INSERT INTO Workers (Email, Password, Name, Role) 
    VALUES ('staff.a@ngo.org', 'password123', '員工A', 'staff');
END

IF NOT EXISTS (SELECT 1 FROM Workers WHERE Email = 'staff.b@ngo.org')
BEGIN
    -- 員工B - 負責案例 4, 5, 6
    INSERT INTO Workers (Email, Password, Name, Role) 
    VALUES ('staff.b@ngo.org', 'password123', '員工B', 'staff');
END

IF NOT EXISTS (SELECT 1 FROM Workers WHERE Email = 'supervisor@ngo.org')
BEGIN
    -- 主管 - 可以監督所有案例
    INSERT INTO Workers (Email, Password, Name, Role) 
    VALUES ('supervisor@ngo.org', 'password123', '主管', 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM Workers WHERE Email = 'admin@ngo.org')
BEGIN
    -- 管理員 - 具有所有權限
    INSERT INTO Workers (Email, Password, Name, Role) 
    VALUES ('admin@ngo.org', 'password123', '管理員', 'admin');
END

-- 確保測試案例存在並分配正確的工作人員
-- 首先獲取工作人員ID
DECLARE @StaffA_ID INT = (SELECT WorkerId FROM Workers WHERE Email = 'staff.a@ngo.org');
DECLARE @StaffB_ID INT = (SELECT WorkerId FROM Workers WHERE Email = 'staff.b@ngo.org');
DECLARE @Supervisor_ID INT = (SELECT WorkerId FROM Workers WHERE Email = 'supervisor@ngo.org');

-- 檢查並創建測試案例
IF NOT EXISTS (SELECT 1 FROM Cases WHERE CaseId = 1)
BEGIN
    INSERT INTO Cases (CaseId, Name, Phone, IdentityNumber, Address, Description, WorkerId, Status, CreatedAt, Email, Gender, City, District, DetailAddress) 
    VALUES (1, '測試案例001', '0900-000-001', 'A123456001', '測試地址001', '員工A負責的案例', @StaffA_ID, 'active', GETDATE(), 'case001@test.com', 'Male', '台北市', '中正區', '測試街1號');
END
ELSE
BEGIN
    UPDATE Cases SET WorkerId = @StaffA_ID WHERE CaseId = 1;
END

IF NOT EXISTS (SELECT 1 FROM Cases WHERE CaseId = 2)
BEGIN
    INSERT INTO Cases (CaseId, Name, Phone, IdentityNumber, Address, Description, WorkerId, Status, CreatedAt, Email, Gender, City, District, DetailAddress) 
    VALUES (2, '測試案例002', '0900-000-002', 'A123456002', '測試地址002', '員工A負責的案例', @StaffA_ID, 'active', GETDATE(), 'case002@test.com', 'Female', '台北市', '中正區', '測試街2號');
END
ELSE
BEGIN
    UPDATE Cases SET WorkerId = @StaffA_ID WHERE CaseId = 2;
END

IF NOT EXISTS (SELECT 1 FROM Cases WHERE CaseId = 3)
BEGIN
    INSERT INTO Cases (CaseId, Name, Phone, IdentityNumber, Address, Description, WorkerId, Status, CreatedAt, Email, Gender, City, District, DetailAddress) 
    VALUES (3, '測試案例003', '0900-000-003', 'A123456003', '測試地址003', '員工A負責的案例', @StaffA_ID, 'active', GETDATE(), 'case003@test.com', 'Male', '台北市', '中正區', '測試街3號');
END
ELSE
BEGIN
    UPDATE Cases SET WorkerId = @StaffA_ID WHERE CaseId = 3;
END

IF NOT EXISTS (SELECT 1 FROM Cases WHERE CaseId = 4)
BEGIN
    INSERT INTO Cases (CaseId, Name, Phone, IdentityNumber, Address, Description, WorkerId, Status, CreatedAt, Email, Gender, City, District, DetailAddress) 
    VALUES (4, '測試案例004', '0900-000-004', 'A123456004', '測試地址004', '員工B負責的案例', @StaffB_ID, 'active', GETDATE(), 'case004@test.com', 'Female', '台北市', '中正區', '測試街4號');
END
ELSE
BEGIN
    UPDATE Cases SET WorkerId = @StaffB_ID WHERE CaseId = 4;
END

IF NOT EXISTS (SELECT 1 FROM Cases WHERE CaseId = 5)
BEGIN
    INSERT INTO Cases (CaseId, Name, Phone, IdentityNumber, Address, Description, WorkerId, Status, CreatedAt, Email, Gender, City, District, DetailAddress) 
    VALUES (5, '測試案例005', '0900-000-005', 'A123456005', '測試地址005', '員工B負責的案例', @StaffB_ID, 'active', GETDATE(), 'case005@test.com', 'Male', '台北市', '中正區', '測試街5號');
END
ELSE
BEGIN
    UPDATE Cases SET WorkerId = @StaffB_ID WHERE CaseId = 5;
END

IF NOT EXISTS (SELECT 1 FROM Cases WHERE CaseId = 6)
BEGIN
    INSERT INTO Cases (CaseId, Name, Phone, IdentityNumber, Address, Description, WorkerId, Status, CreatedAt, Email, Gender, City, District, DetailAddress) 
    VALUES (6, '測試案例006', '0900-000-006', 'A123456006', '測試地址006', '員工B負責的案例', @StaffB_ID, 'active', GETDATE(), 'case006@test.com', 'Female', '台北市', '中正區', '測試街6號');
END
ELSE
BEGIN
    UPDATE Cases SET WorkerId = @StaffB_ID WHERE CaseId = 6;
END

-- 創建一些測試用的物資需求
-- 員工A負責的案例的物資需求
IF NOT EXISTS (SELECT 1 FROM RegularSuppliesNeeds WHERE CaseId = 1)
BEGIN
    INSERT INTO RegularSuppliesNeeds (CaseId, SupplyId, Quantity, Status, ApplyDate, Description) 
    VALUES (1, 1, 5, 'pending', GETDATE(), '案例1的物資需求');
END

IF NOT EXISTS (SELECT 1 FROM RegularSuppliesNeeds WHERE CaseId = 2)
BEGIN
    INSERT INTO RegularSuppliesNeeds (CaseId, SupplyId, Quantity, Status, ApplyDate, Description) 
    VALUES (2, 2, 3, 'approved', GETDATE(), '案例2的物資需求');
END

-- 員工B負責的案例的物資需求
IF NOT EXISTS (SELECT 1 FROM RegularSuppliesNeeds WHERE CaseId = 4)
BEGIN
    INSERT INTO RegularSuppliesNeeds (CaseId, SupplyId, Quantity, Status, ApplyDate, Description) 
    VALUES (4, 3, 2, 'pending', GETDATE(), '案例4的物資需求');
END

IF NOT EXISTS (SELECT 1 FROM RegularSuppliesNeeds WHERE CaseId = 5)
BEGIN
    INSERT INTO RegularSuppliesNeeds (CaseId, SupplyId, Quantity, Status, ApplyDate, Description) 
    VALUES (5, 4, 4, 'approved', GETDATE(), '案例5的物資需求');
END

PRINT '測試用戶和數據已成功創建！';
PRINT '測試帳戶：';
PRINT '  員工A: staff.a@ngo.org / password123 (負責案例1,2,3)';
PRINT '  員工B: staff.b@ngo.org / password123 (負責案例4,5,6)';
PRINT '  主管: supervisor@ngo.org / password123 (可監督所有案例)';
PRINT '  管理員: admin@ngo.org / password123 (具有所有權限)';