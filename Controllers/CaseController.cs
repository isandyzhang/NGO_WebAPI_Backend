using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;
using System.Linq;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    /// <summary>
    /// 個案管理控制器
    /// 
    /// 這個控制器負責處理所有與個案相關的 HTTP 請求：
    /// - 獲取個案列表
    /// - 建立新個案
    /// - 更新個案資料
    /// - 刪除個案
    /// - 搜尋個案
    /// - 測試資料庫連接
    /// 
    /// 個案是 NGO 服務的對象，包含基本資料、聯絡方式、地址等資訊
    /// </summary>
    [ApiController]  // 標記這是一個 API 控制器
    [Route("api/[controller]")]  // 路由：api/case
    public class CaseController : ControllerBase
    {
        // 資料庫上下文 - 用來存取資料庫
        private readonly NgoplatformDbContext _context;
        
        // 日誌記錄器 - 用來記錄系統日誌
        private readonly ILogger<CaseController> _logger;

        // 設定 - 用來存取應用程式設定
        private readonly IConfiguration configuration;

        /// <summary>
        /// 建構函式 - 依賴注入
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="configuration">設定</param>
        public CaseController(NgoplatformDbContext context, ILogger<CaseController> logger, IConfiguration config)
        {
            _context = context;  // 注入資料庫上下文
            _logger = logger;    // 注入日誌記錄器
            configuration = config; // 注入設定
        }

        /// <summary>
        /// 驗證台灣身分證字號格式
        /// </summary>
        /// <param name="identityNumber">身分證字號</param>
        /// <returns>驗證結果</returns>
        private (bool IsValid, string ErrorMessage) ValidateTaiwanIdentityNumber(string identityNumber)
        {
            // 檢查是否為空
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return (false, "身分證字號不能為空");
            }

            // 檢查長度
            if (identityNumber.Length != 10)
            {
                return (false, "身分證字號必須為10位數字");
            }

            // 檢查格式：第一個字母 + 9個數字
            if (!Regex.IsMatch(identityNumber, @"^[A-Z][0-9]{9}$"))
            {
                return (false, "身分證字號格式錯誤：應為1個英文字母後接9個數字");
            }

            // 台灣身分證字號驗證規則
            // 英文字母對應的數字
            var letterValues = new Dictionary<char, int>
            {
                {'A', 10}, {'B', 11}, {'C', 12}, {'D', 13}, {'E', 14},
                {'F', 15}, {'G', 16}, {'H', 17}, {'I', 34}, {'J', 18},
                {'K', 19}, {'L', 20}, {'M', 21}, {'N', 22}, {'O', 35},
                {'P', 23}, {'Q', 24}, {'R', 25}, {'S', 26}, {'T', 27},
                {'U', 28}, {'V', 29}, {'W', 32}, {'X', 30}, {'Y', 31}, {'Z', 33}
            };

            char firstLetter = identityNumber[0];
            if (!letterValues.ContainsKey(firstLetter))
            {
                return (false, "身分證字號第一個字母無效");
            }

            // 取得字母對應的數字
            int letterValue = letterValues[firstLetter];
            
            // 計算驗證碼
            int sum = (letterValue / 10) + (letterValue % 10) * 9;
            
            // 加上後9位數字的權重
            for (int i = 1; i < 9; i++)
            {
                int digit = int.Parse(identityNumber[i].ToString());
                sum += digit * (9 - i);
            }
            
            // 加上最後一位數字
            int lastDigit = int.Parse(identityNumber[9].ToString());
            sum += lastDigit;

            // 檢查是否能被10整除
            if (sum % 10 != 0)
            {
                return (false, "身分證字號驗證碼錯誤");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 獲取個案列表（支援分頁和WorkerId過濾）
        /// HTTP GET: /api/case?page=1&pageSize=10&workerId=1
        /// </summary>
        /// <param name="page">頁碼（預設 1）</param>
        /// <param name="pageSize">每頁數量（預設 10）</param>
        /// <param name="workerId">工作人員ID（可選，用於過濾）</param>
        /// <returns>分頁後的個案列表</returns>
        [HttpGet]  // 處理 GET 請求
        public async Task<ActionResult<PagedResponse<CaseResponse>>> GetAllCases(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? workerId = null)
        {
            try
            {
                _logger.LogInformation($"開始獲取個案列表，頁碼: {page}, 每頁數量: {pageSize}");

                // 驗證分頁參數
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // 建立查詢基礎
                var queryable = _context.Cases
                    .Include(c => c.Worker)  // 包含負責工作人員資料
                    .AsQueryable();

                // 如果提供了 workerId，則按 WorkerId 過濾
                if (workerId.HasValue)
                {
                    queryable = queryable.Where(c => c.WorkerId == workerId.Value);
                    _logger.LogInformation($"按 WorkerId {workerId.Value} 過濾個案");
                }

                queryable = queryable.OrderByDescending(c => c.CreatedAt);  // 按建立時間降序排列

                // 計算總數量
                var totalCount = await queryable.CountAsync();

                // 執行分頁查詢
                var casesData = await queryable
                    .Skip((page - 1) * pageSize)  // 跳過前面的頁面
                    .Take(pageSize)  // 只取當前頁面的資料
                    .ToListAsync();

                // 轉換為回應格式
                var cases = casesData.Select(c => new CaseResponse
                {
                    CaseId = c.CaseId,
                    Name = c.Name ?? string.Empty,
                    Phone = c.Phone ?? string.Empty,
                    IdentityNumber = c.IdentityNumber ?? string.Empty,
                    Birthday = c.Birthday?.ToDateTime(TimeOnly.MinValue),
                    Address = $"{c.City ?? ""}{c.District ?? ""}{c.DetailAddress ?? ""}",
                    WorkerId = c.WorkerId ?? 0,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt ?? DateTime.Now,
                    Status = c.Status ?? string.Empty,
                    Email = c.Email,
                    Gender = c.Gender,
                    ProfileImage = c.ProfileImage,
                    City = c.City,
                    District = c.District,
                    DetailAddress = c.DetailAddress,
                    WorkerName = c.Worker?.Name  // 安全存取工作人員姓名
                }).ToList();

                // 計算分頁資訊
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var hasNextPage = page < totalPages;
                var hasPreviousPage = page > 1;

                var response = new PagedResponse<CaseResponse>
                {
                    Data = cases,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage
                };

                _logger.LogInformation($"成功獲取個案列表，共 {totalCount} 筆，當前頁面 {page}/{totalPages}");
                return Ok(response);  // 回傳 200 OK 狀態碼
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個案列表時發生錯誤");
                return StatusCode(500, new { message = "獲取個案列表失敗" });  // 回傳 500 錯誤
            }
        }

        /// <summary>
        /// 根據 ID 獲取特定個案
        /// HTTP GET: /api/case/{id}
        /// </summary>
        /// <param name="id">個案 ID</param>
        /// <returns>特定個案的詳細資料</returns>
        [HttpGet("{id}")]  // 處理 GET 請求，{id} 是路由參數
        public async Task<ActionResult<CaseResponse>> GetCaseById(int id)
        {
            try
            {
                _logger.LogInformation($"開始獲取個案 ID: {id}");

                // 根據 ID 查詢個案，包含工作人員資料
                var caseItem = await _context.Cases
                    .Include(c => c.Worker)
                    .FirstOrDefaultAsync(c => c.CaseId == id);  // 找到第一個符合條件的個案

                // 如果找不到個案，回傳 404 Not Found
                if (caseItem == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return NotFound(new { message = "找不到指定的個案" });
                }

                // 轉換為回應格式
                var response = new CaseResponse
                {
                    CaseId = caseItem.CaseId,
                    Name = caseItem.Name ?? string.Empty,
                    Phone = caseItem.Phone ?? string.Empty,
                    IdentityNumber = caseItem.IdentityNumber ?? string.Empty,
                    Birthday = caseItem.Birthday?.ToDateTime(TimeOnly.MinValue),
                    Address = $"{caseItem.City ?? ""}{caseItem.District ?? ""}{caseItem.DetailAddress ?? ""}",
                    WorkerId = caseItem.WorkerId ?? 0,
                    Description = caseItem.Description,
                    CreatedAt = caseItem.CreatedAt ?? DateTime.Now,
                    Status = caseItem.Status ?? string.Empty,
                    Email = caseItem.Email,
                    Gender = caseItem.Gender,
                    ProfileImage = caseItem.ProfileImage,
                    City = caseItem.City,
                    District = caseItem.District,
                    DetailAddress = caseItem.DetailAddress,
                    WorkerName = caseItem.Worker?.Name  // 使用 null 條件運算子
                };

                _logger.LogInformation($"成功獲取個案 ID: {id}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取個案 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "獲取個案詳情失敗" });
            }
        }

        /// <summary>
        /// 搜尋個案（支援分頁和WorkerId過濾）
        /// HTTP GET: /api/case/search?query=關鍵字&page=1&pageSize=10&workerId=1
        /// </summary>
        /// <param name="query">搜尋關鍵字</param>
        /// <param name="page">頁碼（預設 1）</param>
        /// <param name="pageSize">每頁數量（預設 10）</param>
        /// <param name="workerId">工作人員ID（可選，用於過濾）</param>
        /// <returns>搜尋結果和分頁資訊</returns>
        [HttpGet("search")]  // 處理 GET 請求：/api/case/search
        public async Task<ActionResult<PagedResponse<CaseResponse>>> SearchCases(
            [FromQuery] string? query,  // 從 URL 查詢參數取得
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? workerId = null)
        {
            try
            {
                _logger.LogInformation($"開始搜尋個案，關鍵字: {query}, 頁碼: {page}, 每頁數量: {pageSize}");

                // 驗證分頁參數
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // 建立查詢基礎
                var queryable = _context.Cases
                    .Include(c => c.Worker)
                    .AsQueryable();  // 轉換為可查詢物件

                // 如果提供了 workerId，則按 WorkerId 過濾
                if (workerId.HasValue)
                {
                    queryable = queryable.Where(c => c.WorkerId == workerId.Value);
                    _logger.LogInformation($"按 WorkerId {workerId.Value} 過濾搜尋結果");
                }

                // 如果有搜尋關鍵字，進行模糊搜尋
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryable = queryable.Where(c =>
                        (c.Name != null && c.Name.Contains(query)) ||           // 姓名包含關鍵字
                        (c.Phone != null && c.Phone.Contains(query)) ||          // 電話包含關鍵字
                        (c.IdentityNumber != null && c.IdentityNumber.Contains(query)) || // 身分證字號包含關鍵字
                        (c.Email != null && c.Email.Contains(query)) ||          // 電子郵件包含關鍵字
                        (c.Description != null && c.Description.Contains(query)) ||    // 描述包含關鍵字
                        (c.City != null && c.City.Contains(query)) ||           // 城市包含關鍵字
                        (c.District != null && c.District.Contains(query))          // 區域包含關鍵字
                    );
                }

                // 計算總數量
                var totalCount = await queryable.CountAsync();
                
                // 執行分頁查詢
                var casesData = await queryable
                    .OrderByDescending(c => c.CreatedAt)  // 按建立時間降序排列
                    .Skip((page - 1) * pageSize)  // 跳過前面的頁面
                    .Take(pageSize)  // 只取當前頁面的資料
                    .ToListAsync();

                // 轉換為回應格式
                var cases = casesData.Select(c => new CaseResponse
                {
                    CaseId = c.CaseId,
                    Name = c.Name ?? string.Empty,
                    Phone = c.Phone ?? string.Empty,
                    IdentityNumber = c.IdentityNumber ?? string.Empty,
                    Birthday = c.Birthday?.ToDateTime(TimeOnly.MinValue),
                    Address = $"{c.City ?? ""}{c.District ?? ""}{c.DetailAddress ?? ""}",
                    WorkerId = c.WorkerId ?? 0,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt ?? DateTime.Now,
                    Status = c.Status ?? string.Empty,
                    Email = c.Email,
                    Gender = c.Gender,
                    ProfileImage = c.ProfileImage,
                    City = c.City,
                    District = c.District,
                    DetailAddress = c.DetailAddress,
                    WorkerName = c.Worker?.Name
                }).ToList();

                // 計算分頁資訊
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var hasNextPage = page < totalPages;
                var hasPreviousPage = page > 1;

                var response = new PagedResponse<CaseResponse>
                {
                    Data = cases,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage
                };

                _logger.LogInformation($"搜尋完成，找到 {totalCount} 個個案，當前頁面 {page}/{totalPages}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋個案時發生錯誤");
                return StatusCode(500, new { message = "搜尋個案失敗" });
            }
        }

        /// <summary>
        /// 上傳個案圖片到 Azure Blob Storage
        /// </summary>
        [HttpPost("upload/profile-image")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> UploadProfileImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "請選擇圖片檔案" });
                }

                // 驗證檔案類型
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "只支援 JPG、PNG、GIF 格式的圖片" });
                }

                // 驗證檔案大小 (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "圖片檔案大小不能超過 5MB" });
                }

                // 從配置中獲取 Azure Storage 設定
                var connectionString = configuration.GetConnectionString("AzureStorage");
                var containerName = configuration.GetValue<string>("AzureStorage:ContainerName");
                var casePhotosFolder = configuration.GetValue<string>("AzureStorage:CasePhotosFolder");

                if (string.IsNullOrEmpty(connectionString))
                {
                    return StatusCode(500, new { message = "Azure Storage 連接字串未配置" });
                }

                // 建立 Azure Blob Service Client
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // 確保容器存在
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // 生成唯一的檔案名稱
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var blobName = $"{casePhotosFolder}{fileName}";

                // 獲取 Blob Client
                var blobClient = containerClient.GetBlobClient(blobName);

                // 設定 Blob 的 Content-Type
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                };

                // 上傳檔案到 Azure Blob Storage
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders
                    });
                }

                // 回傳 Azure Blob URL
                var imageUrl = blobClient.Uri.ToString();
                
                _logger.LogInformation($"個案圖片上傳成功: {fileName}, URL: {imageUrl}");

                return Ok(new { imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "個案圖片上傳失敗");
                return StatusCode(500, new { message = "個案圖片上傳失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 建立新個案
        /// HTTP POST: /api/case
        /// </summary>
        /// <param name="request">建立個案的請求資料</param>
        /// <returns>新建立的個案資料</returns>
        [HttpPost]  // 處理 POST 請求
        public async Task<ActionResult<CaseResponse>> CreateCase([FromBody] CreateCaseRequest request)
        {
            try
            {
                _logger.LogInformation($"開始建立新個案，姓名: {request.Name}");

                // 檢查身分證字號是否已存在（防止重複建立）
                var existingCase = await _context.Cases
                    .FirstOrDefaultAsync(c => c.IdentityNumber == request.IdentityNumber);

                if (existingCase != null)
                {
                    _logger.LogWarning($"身分證字號 {request.IdentityNumber} 已存在");
                    return BadRequest(new { message = "此身分證字號已存在" });  // 回傳 400 錯誤
                }

                // 驗證身分證字號格式
                var validationResult = ValidateTaiwanIdentityNumber(request.IdentityNumber);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"身分證字號 {request.IdentityNumber} 格式錯誤: {validationResult.ErrorMessage}");
                    return BadRequest(new { message = validationResult.ErrorMessage });
                }

                // 建立新的個案實體
                var newCase = new Case
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    IdentityNumber = request.IdentityNumber,
                    Birthday = request.Birthday.HasValue ? DateOnly.FromDateTime(request.Birthday.Value) : null,
                    WorkerId = request.WorkerId ?? 1,  // 使用請求中的WorkerId，若無則預設為1
                    Description = request.Description,
                    CreatedAt = DateTime.Now,  // 設定建立時間為當前時間
                    Status = "active",  // 新個案預設為啟用狀態
                    Email = request.Email,
                    Gender = request.Gender,
                    ProfileImage = request.ProfileImage,
                    City = request.City,
                    District = request.District,
                    DetailAddress = request.DetailAddress
                };

                // 將新個案加入資料庫
                _context.Cases.Add(newCase);
                await _context.SaveChangesAsync();  // 儲存變更

                // 重新查詢以獲取完整資料（包含工作人員資訊）
                var createdCase = await _context.Cases
                    .Include(c => c.Worker)
                    .FirstOrDefaultAsync(c => c.CaseId == newCase.CaseId);

                // 轉換為回應格式
                var response = new CaseResponse
                {
                    CaseId = createdCase!.CaseId,
                    Name = createdCase.Name ?? string.Empty,
                    Phone = createdCase.Phone ?? string.Empty,
                    IdentityNumber = createdCase.IdentityNumber ?? string.Empty,
                    Birthday = createdCase.Birthday?.ToDateTime(TimeOnly.MinValue),
                    Address = $"{createdCase.City ?? ""}{createdCase.District ?? ""}{createdCase.DetailAddress ?? ""}",
                    WorkerId = createdCase.WorkerId ?? 0,
                    Description = createdCase.Description,
                    CreatedAt = createdCase.CreatedAt ?? DateTime.Now,
                    Status = createdCase.Status ?? string.Empty,
                    Email = createdCase.Email,
                    Gender = createdCase.Gender,
                    ProfileImage = createdCase.ProfileImage,
                    City = createdCase.City,
                    District = createdCase.District,
                    DetailAddress = createdCase.DetailAddress,
                    WorkerName = createdCase.Worker?.Name
                };

                _logger.LogInformation($"成功建立個案 ID: {newCase.CaseId}");
                
                // 回傳 201 Created 狀態碼，並提供新資源的位置
                return CreatedAtAction(nameof(GetCaseById), new { id = newCase.CaseId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立個案時發生錯誤");
                return StatusCode(500, new { message = "建立個案失敗" });
            }
        }

        /// <summary>
        /// 更新個案資料
        /// HTTP PUT: /api/case/{id}
        /// </summary>
        /// <param name="id">要更新的個案 ID</param>
        /// <param name="request">更新的資料</param>
        /// <returns>更新結果</returns>
        [HttpPut("{id}")]  // 處理 PUT 請求
        public async Task<ActionResult> UpdateCase(int id, [FromBody] UpdateCaseRequest request)
        {
            try
            {
                _logger.LogInformation($"開始更新個案 ID: {id}");

                // 根據 ID 查找個案
                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return NotFound(new { message = "找不到指定的個案" });
                }

                // 只更新有提供的欄位（部分更新）
                if (request.Name != null) caseItem.Name = request.Name;
                if (request.Phone != null) caseItem.Phone = request.Phone;
                if (request.IdentityNumber != null) 
                {
                    // 驗證身分證字號格式
                    var validationResult = ValidateTaiwanIdentityNumber(request.IdentityNumber);
                    if (!validationResult.IsValid)
                    {
                        _logger.LogWarning($"身分證字號 {request.IdentityNumber} 格式錯誤: {validationResult.ErrorMessage}");
                        return BadRequest(new { message = validationResult.ErrorMessage });
                    }
                    caseItem.IdentityNumber = request.IdentityNumber;
                }
                if (request.Birthday.HasValue) caseItem.Birthday = DateOnly.FromDateTime(request.Birthday.Value);
                // Address 欄位已被移除，現在由 City + District + DetailAddress 組成
                if (request.Description != null) caseItem.Description = request.Description;
                if (request.Status != null) caseItem.Status = request.Status;
                if (request.Email != null) caseItem.Email = request.Email;
                if (request.Gender != null) caseItem.Gender = request.Gender;
                if (request.ProfileImage != null) caseItem.ProfileImage = request.ProfileImage;
                if (request.City != null) caseItem.City = request.City;
                if (request.District != null) caseItem.District = request.District;
                if (request.DetailAddress != null) caseItem.DetailAddress = request.DetailAddress;

                // 儲存變更
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功更新個案 ID: {id}");
                return NoContent();  // 回傳 204 No Content（更新成功但沒有內容回傳）
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新個案 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "更新個案失敗" });
            }
        }

        /// <summary>
        /// 刪除個案
        /// HTTP DELETE: /api/case/{id}
        /// </summary>
        /// <param name="id">要刪除的個案 ID</param>
        /// <returns>刪除結果</returns>
        [HttpDelete("{id}")]  // 處理 DELETE 請求
        public async Task<ActionResult> DeleteCase(int id)
        {
            try
            {
                _logger.LogInformation($"開始刪除個案 ID: {id}");

                // 根據 ID 查找個案
                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return NotFound(new { message = "找不到指定的個案" });
                }

                // 從資料庫中移除個案
                _context.Cases.Remove(caseItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功刪除個案 ID: {id}");
                return NoContent();  // 回傳 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除個案 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "刪除個案失敗" });
            }
        }


    }

    // ==================== API 請求/回應模型 ====================

    /// <summary>
    /// 建立個案的請求模型
    /// 定義前端傳送建立個案時需要的資料格式
    /// </summary>
    public class CreateCaseRequest
    {
        public string Name { get; set; } = string.Empty;           // 個案姓名（必填）
        public string Phone { get; set; } = string.Empty;          // 聯絡電話（必填）
        public string IdentityNumber { get; set; } = string.Empty; // 身分證字號（必填）
        public DateTime? Birthday { get; set; }                    // 生日（可選）
        public int? WorkerId { get; set; }                         // 負責工作人員ID（可選）
        // Address 欄位已移除，現在由 City + District + DetailAddress 組成
        public string? Description { get; set; }                   // 描述（可選）
        public string? Email { get; set; }                         // 電子郵件（可選）
        public string? Gender { get; set; }                        // 性別（可選）
        public string? ProfileImage { get; set; }                  // 大頭照（可選）
        public string? City { get; set; }                          // 城市（可選）
        public string? District { get; set; }                      // 區域（可選）
        public string? DetailAddress { get; set; }                 // 詳細地址（可選）
    }

    /// <summary>
    /// 更新個案的請求模型
    /// 所有欄位都是可選的，只更新有提供的欄位
    /// </summary>
    public class UpdateCaseRequest
    {
        public string? Name { get; set; }                          // 個案姓名
        public string? Phone { get; set; }                         // 聯絡電話
        public string? IdentityNumber { get; set; }                // 身分證字號
        public DateTime? Birthday { get; set; }                    // 生日
        // Address 欄位已移除，現在由 City + District + DetailAddress 組成
        public string? Description { get; set; }                   // 描述
        public string? Status { get; set; }                        // 狀態
        public string? Email { get; set; }                         // 電子郵件
        public string? Gender { get; set; }                        // 性別
        public string? ProfileImage { get; set; }                  // 大頭照
        public string? City { get; set; }                          // 城市
        public string? District { get; set; }                      // 區域
        public string? DetailAddress { get; set; }                 // 詳細地址
    }

    /// <summary>
    /// 個案回應模型
    /// 定義回傳給前端的個案資料格式
    /// </summary>
    public class CaseResponse
    {
        public int CaseId { get; set; }                            // 個案 ID
        public string Name { get; set; } = string.Empty;           // 個案姓名
        public string Phone { get; set; } = string.Empty;          // 聯絡電話
        public string IdentityNumber { get; set; } = string.Empty; // 身分證字號
        public DateTime? Birthday { get; set; }                    // 生日
        public string Address { get; set; } = string.Empty;        // 地址
        public int WorkerId { get; set; }                          // 負責工作人員 ID
        public string? Description { get; set; }                   // 描述
        public DateTime CreatedAt { get; set; }                    // 建立時間
        public string Status { get; set; } = string.Empty;         // 狀態
        public string? Email { get; set; }                         // 電子郵件
        public string? Gender { get; set; }                        // 性別
        public string? ProfileImage { get; set; }                  // 大頭照
        public string? City { get; set; }                          // 城市
        public string? District { get; set; }                      // 區域
        public string? DetailAddress { get; set; }                 // 詳細地址
        public string? WorkerName { get; set; }                    // 工作人員姓名（關聯查詢）
    }

    /// <summary>
    /// 分頁回應模型
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();         // 分頁資料
        public int Page { get; set; }                              // 當前頁碼
        public int PageSize { get; set; }                          // 每頁數量
        public int TotalCount { get; set; }                        // 總數量
        public int TotalPages { get; set; }                        // 總頁數
        public bool HasNextPage { get; set; }                      // 是否有下一頁
        public bool HasPreviousPage { get; set; }                  // 是否有上一頁
    }
} 