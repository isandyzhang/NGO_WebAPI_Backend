using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;

namespace NGO_WebAPI_Backend.Controllers.SupplyManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencySupplyNeedController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<EmergencySupplyNeedController> _logger;
        private readonly IConfiguration _configuration;

        public EmergencySupplyNeedController(NgoplatformDbContext context, ILogger<EmergencySupplyNeedController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 獲取所有緊急物資需求
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmergencySupplyNeedResponse>>> GetEmergencySupplyNeeds()
        {
            try
            {
                _logger.LogInformation("開始獲取緊急物資需求列表");

                // 完全避免關聯查詢，使用投影來避免 SupplyId 問題
                var emergencyNeedsData = await _context.EmergencySupplyNeeds
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.EmergencyNeedId,
                        e.CaseId,
                        e.WorkerId,
                        e.Quantity,
                        e.CollectedQuantity,
                        e.SupplyName,
                        e.Status,
                        e.Priority,
                        e.Description,
                        e.ImageUrl,
                        e.CreatedDate,
                        e.UpdatedDate,
                        CaseName = e.Case != null ? e.Case.Name : null,
                        WorkerName = e.Worker != null ? e.Worker.Name : null
                    })
                    .ToListAsync();

                var response = emergencyNeedsData.Select(e => new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = e.EmergencyNeedId,
                    ItemName = e.SupplyName ?? "未知物品",
                    Category = "緊急物資",
                    Quantity = e.Quantity ?? 0,
                    CollectedQuantity = e.CollectedQuantity ?? 0,
                    Unit = "個",
                    RequestedBy = e.WorkerName ?? "未知申請人",
                    RequestDate = e.CreatedDate ?? DateTime.Now,
                    Status = e.Status ?? "pending",
                    Priority = e.Priority ?? "medium",
                    Description = e.Description ?? "",
                    ImageUrl = e.ImageUrl ?? "",
                    CaseName = e.CaseName ?? "未知個案",
                    CaseId = e.CaseId?.ToString() ?? "未知",
                    Matched = e.Status == "approved",
                    EmergencyReason = e.Description ?? "緊急物資需求"
                }).ToList();

                _logger.LogInformation($"成功獲取 {response.Count} 個緊急物資需求");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取緊急物資需求失敗");
                return StatusCode(500, new { message = "獲取緊急物資需求失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 獲取緊急物資需求統計
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<EmergencySupplyNeedStatistics>> GetEmergencySupplyNeedStatistics()
        {
            try
            {
                _logger.LogInformation("開始獲取緊急物資需求統計");

                // 完全避免關聯查詢，只取基本資料
                var emergencyNeeds = await _context.EmergencySupplyNeeds
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.EmergencyNeedId,
                        e.CaseId,
                        e.WorkerId,
                        e.Quantity,
                        e.CollectedQuantity,
                        e.SupplyName,
                        e.Status,
                        e.Priority,
                        e.Description,
                        e.ImageUrl,
                        e.CreatedDate,
                        e.UpdatedDate
                    })
                    .ToListAsync();

                var statistics = new EmergencySupplyNeedStatistics
                {
                    TotalRequests = emergencyNeeds.Count,
                    PendingRequests = emergencyNeeds.Count(e => e.Status == "pending"),
                    ApprovedRequests = emergencyNeeds.Count(e => e.Status == "approved"),
                    RejectedRequests = emergencyNeeds.Count(e => e.Status == "rejected"),
                    CompletedRequests = emergencyNeeds.Count(e => e.Status == "completed"),
                    HighPriorityRequests = emergencyNeeds.Count(e => e.Priority == "high"),
                    TotalQuantity = emergencyNeeds.Sum(e => e.Quantity ?? 0),
                    CollectedQuantity = emergencyNeeds.Sum(e => e.CollectedQuantity ?? 0)
                };

                _logger.LogInformation("成功獲取緊急物資需求統計");
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取緊急物資需求統計失敗");
                return StatusCode(500, new { message = "獲取緊急物資需求統計失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 根據ID獲取緊急物資需求
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmergencySupplyNeedResponse>> GetEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始獲取緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds
                    .Include(e => e.Case)
                    .Include(e => e.Worker)
                    .FirstOrDefaultAsync(e => e.EmergencyNeedId == id);

                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(new { message = "找不到指定的緊急物資需求" });
                }

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = emergencyNeed.EmergencyNeedId,
                    ItemName = emergencyNeed.SupplyName ?? "未知物品",
                    Category = "緊急物資",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    CollectedQuantity = emergencyNeed.CollectedQuantity ?? 0,
                    Unit = "個",
                    RequestedBy = emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.CreatedDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    Priority = emergencyNeed.Priority ?? "medium",
                    Description = emergencyNeed.Description ?? "",
                    ImageUrl = emergencyNeed.ImageUrl ?? "",
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Status == "approved",
                    EmergencyReason = emergencyNeed.Description ?? "緊急物資需求"
                };

                _logger.LogInformation($"成功獲取緊急物資需求 ID: {id}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, new { message = "獲取緊急物資需求失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 創建緊急物資需求
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EmergencySupplyNeedResponse>> CreateEmergencySupplyNeed([FromBody] CreateEmergencySupplyNeedRequest request)
        {
            try
            {
                _logger.LogInformation("開始創建緊急物資需求");

                if (request == null)
                {
                    return BadRequest(new { message = "請求資料不能為空" });
                }

                var emergencyNeed = new EmergencySupplyNeed
                {
                    CaseId = request.CaseId,
                    WorkerId = request.WorkerId,
                    SupplyName = request.SupplyName,
                    Quantity = request.Quantity,
                    CollectedQuantity = 0,
                    Status = request.Status ?? "pending",
                    Priority = request.Priority ?? "medium",
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.EmergencySupplyNeeds.Add(emergencyNeed);
                await _context.SaveChangesAsync();

                // 重新載入關聯資料
                await _context.Entry(emergencyNeed)
                    .Reference(e => e.Case)
                    .LoadAsync();
                await _context.Entry(emergencyNeed)
                    .Reference(e => e.Worker)
                    .LoadAsync();

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = emergencyNeed.EmergencyNeedId,
                    ItemName = emergencyNeed.SupplyName ?? "未知物品",
                    Category = "緊急物資",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    CollectedQuantity = emergencyNeed.CollectedQuantity ?? 0,
                    Unit = "個",
                    RequestedBy = emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.CreatedDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    Priority = emergencyNeed.Priority ?? "medium",
                    Description = emergencyNeed.Description ?? "",
                    ImageUrl = emergencyNeed.ImageUrl ?? "",
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Status == "approved",
                    EmergencyReason = emergencyNeed.Description ?? "緊急物資需求"
                };

                _logger.LogInformation($"成功創建緊急物資需求 ID: {emergencyNeed.EmergencyNeedId}");
                return CreatedAtAction(nameof(GetEmergencySupplyNeed), new { id = emergencyNeed.EmergencyNeedId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建緊急物資需求失敗");
                return StatusCode(500, new { message = "創建緊急物資需求失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 上傳緊急物資需求圖片到 Azure Blob Storage
        /// </summary>
        [HttpPost("upload/image")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
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
                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerName = _configuration.GetValue<string>("AzureStorage:ContainerName");
                var emergencySupplyFolder = _configuration.GetValue<string>("AzureStorage:EmergencySupplyFolder") ?? "emergency-supply/";

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
                var blobName = $"{emergencySupplyFolder}{fileName}";

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
                
                _logger.LogInformation($"緊急物資需求圖片上傳成功: {fileName}, URL: {imageUrl}");

                return Ok(new { imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "緊急物資需求圖片上傳失敗");
                return StatusCode(500, new { message = "緊急物資需求圖片上傳失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 創建測試數據（僅供開發測試使用）
        /// </summary>
        [HttpPost("test-data")]
        public async Task<ActionResult> CreateTestData()
        {
            try
            {
                _logger.LogInformation("開始創建測試數據");

                // 檢查是否已有測試數據
                var existingData = await _context.EmergencySupplyNeeds.AnyAsync();
                if (existingData)
                {
                    return Ok(new { message = "測試數據已存在" });
                }

                // 先獲取一些現有的Case、Worker數據
                var cases = await _context.Cases.Take(3).ToListAsync();
                var workers = await _context.Workers.Take(3).ToListAsync();

                if (!cases.Any() || !workers.Any())
                {
                    return BadRequest(new { message = "缺少必要的基礎數據（Case、Worker）" });
                }

                var testData = new List<EmergencySupplyNeed>
                {
                    new EmergencySupplyNeed
                    {
                        CaseId = cases[0].CaseId,
                        WorkerId = workers[0].WorkerId,
                        SupplyName = "緊急毛毯",
                        Quantity = 5,
                        CollectedQuantity = 0,
                        Status = "pending",
                        Priority = "high",
                        Description = "個案家庭因火災需要緊急毛毯",
                        CreatedDate = DateTime.Now.AddDays(-2),
                        UpdatedDate = DateTime.Now.AddDays(-2)
                    },
                    new EmergencySupplyNeed
                    {
                        CaseId = cases.Count > 1 ? cases[1].CaseId : cases[0].CaseId,
                        WorkerId = workers.Count > 1 ? workers[1].WorkerId : workers[0].WorkerId,
                        SupplyName = "急用藥品",
                        Quantity = 3,
                        CollectedQuantity = 0,
                        Status = "approved",
                        Priority = "high",
                        Description = "糖尿病患者急需胰島素",
                        CreatedDate = DateTime.Now.AddDays(-1),
                        UpdatedDate = DateTime.Now.AddDays(-1)
                    },
                    new EmergencySupplyNeed
                    {
                        CaseId = cases.Count > 2 ? cases[2].CaseId : cases[0].CaseId,
                        WorkerId = workers.Count > 2 ? workers[2].WorkerId : workers[0].WorkerId,
                        SupplyName = "緊急食物包",
                        Quantity = 10,
                        CollectedQuantity = 0,
                        Status = "pending",
                        Priority = "medium",
                        Description = "失業家庭需要緊急食物援助",
                        CreatedDate = DateTime.Now.AddHours(-6),
                        UpdatedDate = DateTime.Now.AddHours(-6)
                    }
                };

                _context.EmergencySupplyNeeds.AddRange(testData);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功創建 {testData.Count} 筆測試數據");
                return Ok(new { message = $"成功創建 {testData.Count} 筆測試數據" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建測試數據失敗");
                return StatusCode(500, new { message = "創建測試數據失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 批准緊急物資需求
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<ActionResult> ApproveEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始批准緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(new { message = "找不到指定的緊急物資需求" });
                }

                emergencyNeed.Status = "approved";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功批准緊急物資需求 ID: {id}");
                return Ok(new { message = "緊急物資需求已批准" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批准緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, new { message = "批准緊急物資需求失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 拒絕緊急物資需求
        /// </summary>
        [HttpPut("{id}/reject")]
        public async Task<ActionResult> RejectEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始拒絕緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(new { message = "找不到指定的緊急物資需求" });
                }

                emergencyNeed.Status = "rejected";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功拒絕緊急物資需求 ID: {id}");
                return Ok(new { message = "緊急物資需求已拒絕" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"拒絕緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, new { message = "拒絕緊急物資需求失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 刪除緊急物資需求
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmergencySupplyNeed(int id)
        {
            try
            {
                _logger.LogInformation($"開始刪除緊急物資需求 ID: {id}");

                var emergencyNeed = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (emergencyNeed == null)
                {
                    _logger.LogWarning($"找不到緊急物資需求 ID: {id}");
                    return NotFound(new { message = "找不到指定的緊急物資需求" });
                }

                _context.EmergencySupplyNeeds.Remove(emergencyNeed);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功刪除緊急物資需求 ID: {id}");
                return Ok(new { message = "緊急物資需求已刪除" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除緊急物資需求 ID: {id} 失敗");
                return StatusCode(500, new { message = "刪除緊急物資需求失敗", error = ex.Message });
            }
        }
    }

    // DTO 類別
    public class EmergencySupplyNeedResponse
    {
        public int EmergencyNeedId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int CollectedQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CaseName { get; set; } = string.Empty;
        public string CaseId { get; set; } = string.Empty;
        public bool Matched { get; set; }
        public string EmergencyReason { get; set; } = string.Empty;
    }

    public class CreateEmergencySupplyNeedRequest
    {
        public int? CaseId { get; set; }
        public int? WorkerId { get; set; }
        public string? SupplyName { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class EmergencySupplyNeedStatistics
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int HighPriorityRequests { get; set; }
        public int TotalQuantity { get; set; }
        public int CollectedQuantity { get; set; }
    }
} 