# Controllers 架構說明

本專案的 Controllers 已按功能分類整理，採用模組化的資料夾結構：

## 📁 資料夾結構

### 🏠 **CaseManagement** - 個案管理
負責個案相關的所有功能
- `CaseController.cs` - 舊版個案管理 API
- `CaseNewController.cs` - 新架構個案管理 API（推薦使用）
- `CaseOrderController.cs` - 個案訂單管理

### 📦 **SupplyManagement** - 物資管理
處理所有物資供需和配送功能
- `SupplyController.cs` - 物資基本管理
- `EmergencySupplyNeedController.cs` - 緊急物資需求
- `RegularSuppliesNeedController.cs` - 常駐物資需求
- `RegularSupplyMatchController.cs` - 物資配對
- `RegularDistributionBatchController.cs` - 配送批次管理

### 👥 **UserManagement** - 使用者管理
處理帳號、認證和權限相關功能
- `AuthController.cs` - 身份認證
- `AccountController.cs` - 帳號管理
- `WorkerController.cs` - 工作人員管理
- `UserOrderController.cs` - 使用者訂單
- `UserOrderDetailController.cs` - 訂單詳細資料

### 🎯 **ActivityManagement** - 活動管理
負責活動和行程相關功能
- `ActivityController.cs` - 活動管理
- `RegistrationReviewController.cs` - 報名審核
- `ScheduleController.cs` - 行程安排

### ⚙️ **SystemManagement** - 系統管理
系統核心功能和工具
- `DashboardController.cs` - 儀表板
- `AIController.cs` - AI 功能
- `ImageGenerationController.cs` - 圖片生成
- `SpeechController.cs` - 語音處理

## 🏗️ 架構優勢

### 1. **模組化設計**
- 每個功能模組獨立管理
- 便於團隊分工協作
- 降低模組間的耦合度

### 2. **維護性提升**
- 快速定位相關功能
- 減少程式碼查找時間
- 便於新成員理解專案結構

### 3. **擴展性良好**
- 新功能可以獨立添加到對應模組
- 不影響其他模組的運作
- 支援微服務架構演進

## 📋 命名空間規範

```csharp
// 範例：個案管理相關 Controller
namespace NGO_WebAPI_Backend.Controllers.CaseManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaseNewController : ControllerBase
    {
        // ... Controller 實作
    }
}
```

## 🔄 API 路由

所有 API 路由保持不變：
- `/api/casenew` - 新架構個案管理
- `/api/case` - 舊版個案管理
- `/api/supply` - 物資管理
- `/api/auth` - 身份認證
- 等等...

## 📝 最佳實務

1. **新功能開發**：優先使用新架構的 Controller（如 CaseNewController）
2. **向後相容**：保留舊版 API 以確保現有系統正常運作
3. **驗證機制**：新 Controller 已整合 FluentValidation 驗證
4. **統一回應**：使用 ApiResponse<T> 統一回應格式

## 🚀 未來規劃

- 逐步將舊版 API 遷移到新架構
- 進一步拆分大型 Controller
- 引入更多設計模式（CQRS、MediatR）
- 支援 API 版本控制