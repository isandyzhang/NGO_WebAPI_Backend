# NGO 平台後端 API

NGO 平台的後端 API 服務，使用 ASP.NET Core 9.0 開發，提供完整的個案管理、活動管理、物資管理等功能。

## 🏗️ 架構設計

### 三層架構
- **Controller 層**：處理 HTTP 請求和回應
- **Service 層**：處理業務邏輯
- **Repository 層**：處理資料存取

### 核心特性
- ✅ 統一的 `ApiResponse<T>` 回應格式
- ✅ FluentValidation 資料驗證
- ✅ Entity Framework Core 資料存取
- ✅ Azure 服務整合（Storage, OpenAI, Speech）
- ✅ CORS 跨域支援
- ✅ 完整的錯誤處理機制

## 📋 功能模組

### 🧑‍🤝‍🧑 個案管理 (Case Management)
- 個案 CRUD 操作
- 分頁查詢和搜尋
- 個案圖片上傳 (Azure Blob Storage)
- 身分證字號驗證

### 📅 活動管理 (Activity Management)
- 活動建立和管理
- 活動報名系統
- 行程安排

### 📦 物資管理 (Supply Management)
- 緊急物資需求管理
- 定期物資配送
- 物資媒合系統

### 👥 使用者管理 (User Management)
- 工作人員帳號管理
- 登入驗證
- 權限控制

### 🔧 系統管理 (System Management)
- Dashboard 統計資料
- AI 文字生成 (Azure OpenAI)
- 圖片生成 (DALL-E)
- 語音轉文字 (Azure Speech)

## 🚀 快速開始

### 環境需求
- .NET 9.0 SDK
- SQL Server
- Azure 訂閱 (Storage, OpenAI, Speech Services)

### 安裝步驟

1. **複製專案**
```bash
git clone https://github.com/isandyzhang/NGO_WebAPI_Backend.git
cd NGO_WebAPI_Backend
```

2. **安裝套件**
```bash
dotnet restore
```

3. **設定資料庫連線**
建立 `appsettings.Development.json` 檔案：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=your-database;User ID=your-username;Password=your-password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=your-account;AccountKey=your-key;EndpointSuffix=core.windows.net"
  },
  "AzureStorage": {
    "ContainerName": "ngo",
    "CasePhotosFolder": "case_photos/",
    "ActivityImagesFolder": "activity_images/",
    "EmergencySupplyFolder": "emergency_supply/",
    "AudioFolder": "case_audio/"
  },
  "AzureSpeech": {
    "Key": "your-speech-key",
    "Region": "eastasia"
  },
  "AzureOpenAI": {
    "Endpoint": "your-openai-endpoint",
    "ApiKey": "your-openai-key",
    "DeploymentName": "gpt-4",
    "DalleDeploymentName": "dall-e-3"
  }
}
```

4. **執行資料庫遷移**
```bash
dotnet ef database update
```

5. **啟動服務**
```bash
dotnet run
```

API 將在 `http://localhost:5264` 上運行。

## 📚 API 文件

### 個案管理 API

| 方法 | 端點 | 說明 |
|------|------|------|
| GET | `/api/case` | 取得個案列表（支援分頁） |
| GET | `/api/case/{id}` | 取得特定個案詳情 |
| POST | `/api/case` | 建立新個案 |
| PUT | `/api/case/{id}` | 更新個案資料 |
| DELETE | `/api/case/{id}` | 刪除個案 |
| GET | `/api/case/search` | 搜尋個案 |
| POST | `/api/case/upload/profile-image` | 上傳個案圖片 |

### 工作人員 API

| 方法 | 端點 | 說明 |
|------|------|------|
| GET | `/api/worker` | 取得工作人員列表 |
| GET | `/api/worker/by-email/{email}` | 根據 Email 查詢工作人員 |
| POST | `/api/worker/login` | 工作人員登入 |

### API 回應格式

所有 API 都使用統一的回應格式：

```json
{
  "success": true,
  "message": "操作成功",
  "data": { ... },
  "timestamp": "2025-01-01T00:00:00Z"
}
```

分頁 API 回應格式：
```json
{
  "success": true,
  "message": "獲取資料成功",
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "timestamp": "2025-01-01T00:00:00Z"
}
```

## 🔧 開發說明

### 專案結構
```
NGO_WebAPI_Backend/
├── Controllers/           # 控制器
│   ├── CaseManagement/   # 個案管理
│   ├── ActivityManagement/ # 活動管理
│   ├── SupplyManagement/  # 物資管理
│   ├── UserManagement/    # 使用者管理
│   └── SystemManagement/  # 系統管理
├── Models/               # 資料模型
├── DTOs/                 # 資料傳輸物件
├── Services/             # 業務邏輯層
├── Repositories/         # 資料存取層
├── Validators/           # FluentValidation 驗證器
└── Migrations/           # EF Core 遷移檔案
```

### 新增功能流程

1. **建立 DTO**：在 `DTOs/` 目錄下建立資料傳輸物件
2. **建立 Validator**：在 `Validators/` 目錄下建立驗證器
3. **建立 Repository**：在 `Repositories/` 目錄下建立資料存取介面和實作
4. **建立 Service**：在 `Services/` 目錄下建立業務邏輯介面和實作
5. **建立 Controller**：在對應的 `Controllers/` 子目錄下建立控制器
6. **註冊服務**：在 `Program.cs` 中註冊新的服務

### 程式碼規範

- 使用 **PascalCase** 命名 C# 類別、方法、屬性
- 使用 **camelCase** 命名 JSON 回應（已設定自動轉換）
- 所有 API 都要有適當的錯誤處理
- 使用 `ApiResponse<T>` 統一回應格式
- 新增的功能都要有相應的 FluentValidation 驗證

## 🔐 安全性

- 所有敏感資訊都不應提交到版本控制
- 使用 Azure Key Vault 管理生產環境密鑰
- API 支援 CORS，生產環境已限制特定來源
- 輸入資料都經過 FluentValidation 驗證

## 🚀 部署

### Azure App Service 部署

1. 設定 Azure App Service 環境變數
2. 設定正確的連線字串
3. 確保 .NET 9.0 運行時
4. 設定 CORS 允許的前端網址

### 環境變數設定
```
ConnectionStrings__DefaultConnection=your-db-connection-string
ConnectionStrings__AzureStorage=your-storage-connection-string
AzureSpeech__Key=your-speech-key
AzureSpeech__Region=eastasia
AzureOpenAI__Endpoint=your-openai-endpoint
AzureOpenAI__ApiKey=your-openai-key
```

## 🤝 貢獻

1. Fork 專案
2. 建立功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交變更 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 開啟 Pull Request

## 📝 更新日誌

### v2.0.0 (2025-01-01)
- 重構個案管理功能為三層架構
- 新增 FluentValidation 資料驗證
- 統一 API 回應格式
- 修復 Azure 部署問題
- 改善錯誤處理機制

### v1.0.0 (2024-12-01)
- 初始版本發布
- 基本的 CRUD 功能
- Azure 服務整合

## 📞 聯絡資訊

- 開發者：Andy
- Email:isandyzhang@gmail.com
- 專案連結：[https://github.com/isandyzhang/NGO_WebAPI_Backend](https://github.com/isandyzhang/NGO_WebAPI_Backend)
