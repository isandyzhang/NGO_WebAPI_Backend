# 🔧 開發環境設定說明

## 📁 設定檔案結構

本專案使用以下設定檔案：

```
NGO_WebAPI_Backend/
├── appsettings.json              # 基本設定（已提交到 Git）
├── appsettings.Development.json  # 開發環境設定（本地檔案，不提交到 Git）
├── appsettings.template.json     # 設定範本（已提交到 Git）
└── .gitignore                    # Git 忽略規則
```

## 🚀 快速開始

### 1. 複製設定範本
```bash
# 複製範本檔案作為開發環境設定
cp appsettings.template.json appsettings.Development.json
```

### 2. 填入您的設定值
編輯 `appsettings.Development.json` 檔案，填入您的：

- **資料庫連線字串**
- **Azure Storage 連線字串**
- **Azure Speech API 金鑰**
- **Azure OpenAI API 金鑰**

## 🔐 安全性說明

### ✅ 已保護的檔案
- `appsettings.Development.json` - 包含敏感資訊，已加入 `.gitignore`
- `appsettings.Production.json` - 生產環境設定，已加入 `.gitignore`

### ✅ 安全的檔案
- `appsettings.json` - 基本設定，不包含敏感資訊
- `appsettings.template.json` - 設定範本，供團隊參考

## 📋 設定項目說明

### ConnectionStrings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "您的 SQL Server 連線字串",
    "AzureStorage": "您的 Azure Storage 連線字串"
  }
}
```

### Azure Speech
```json
{
  "AzureSpeech": {
    "Key": "您的 Azure Speech API 金鑰",
    "Region": "您的 Azure 區域 (例如: eastasia)"
  }
}
```

### Azure OpenAI
```json
{
  "AzureOpenAI": {
    "Endpoint": "您的 Azure OpenAI 端點",
    "ApiKey": "您的 Azure OpenAI API 金鑰",
    "DeploymentName": "您的 GPT 模型部署名稱",
    "DalleDeploymentName": "您的 DALL-E 模型部署名稱"
  }
}
```

### Azure Storage
```json
{
  "AzureStorage": {
    "ContainerName": "您的 Blob 容器名稱",
    "CasePhotosFolder": "個案照片資料夾路徑",
    "ActivityImagesFolder": "活動圖片資料夾路徑",
    "AudioFolder": "音檔資料夾路徑"
  }
}
```

## 🛠️ 環境變數替代方案

如果您不想使用 `appsettings.Development.json`，也可以使用環境變數：

### Windows
```cmd
set ConnectionStrings__DefaultConnection="您的連線字串"
set AzureSpeech__Key="您的金鑰"
```

### macOS/Linux
```bash
export ConnectionStrings__DefaultConnection="您的連線字串"
export AzureSpeech__Key="您的金鑰"
```

### Docker
```yaml
environment:
  - ConnectionStrings__DefaultConnection=您的連線字串
  - AzureSpeech__Key=您的金鑰
```

## 🔍 驗證設定

啟動應用程式後，檢查以下端點是否正常運作：

- `GET /api/health` - 健康檢查
- `GET /api/case` - 個案 API
- `POST /api/case` - 建立個案

## ⚠️ 注意事項

1. **永遠不要提交敏感資訊到 Git**
2. **定期更新 API 金鑰**
3. **使用強密碼和安全的連線字串**
4. **在生產環境中使用 Azure Key Vault**

## 🆘 常見問題

### Q: 為什麼我的設定沒有生效？
A: 確認檔案名稱是否正確，且位於正確的目錄中。

### Q: 如何在不同環境間切換？
A: 使用 `ASPNETCORE_ENVIRONMENT` 環境變數：
```bash
export ASPNETCORE_ENVIRONMENT=Development
```

### Q: 設定檔案的優先順序？
A: 1. 環境變數 > 2. appsettings.{Environment}.json > 3. appsettings.json 