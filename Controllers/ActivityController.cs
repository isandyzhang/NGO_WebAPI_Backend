using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient; // Added for SqlException

namespace NGO_WebAPI_Backend.Controllers
{
    /// <summary>
    /// 活動管理控制器 - 簡化版本
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<ActivityController> _logger;
        private readonly IConfiguration _configuration;

        public ActivityController(NgoplatformDbContext context, ILogger<ActivityController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 獲取所有活動列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityResponse>>> GetAllActivities()
        {
            try
            {
                _logger.LogInformation("開始獲取所有活動");

                // 檢查數據庫連接
                if (!await _context.Database.CanConnectAsync())
                {
                    _logger.LogError("數據庫連接失敗");
                    return StatusCode(500, new { message = "數據庫連接失敗", error = "Database connection failed" });
                }

                var activitiesData = await _context.Activities
                    .Include(a => a.Worker)
                    .ToListAsync();

                _logger.LogInformation($"從數據庫獲取到 {activitiesData.Count} 個活動");

                var activities = activitiesData.Select(a => new ActivityResponse
                {
                    ActivityId = a.ActivityId,
                    ActivityName = a.ActivityName ?? string.Empty,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    Location = a.Location ?? string.Empty,
                    MaxParticipants = a.MaxParticipants ?? 0,
                    CurrentParticipants = a.CurrentParticipants ?? 0,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    SignupDeadline = a.SignupDeadline,
                    WorkerId = a.WorkerId ?? 0,
                    TargetAudience = a.TargetAudience,
                    Category = a.Category,
                    Status = a.Status ?? string.Empty,
                    WorkerName = a.Worker?.Name
                }).ToList();

                _logger.LogInformation($"成功獲取 {activities.Count} 個活動");
                return Ok(activities);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL 錯誤: {ErrorMessage}", sqlEx.Message);
                return StatusCode(500, new { message = "數據庫查詢失敗", error = sqlEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取活動列表時發生錯誤: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "獲取活動列表失敗", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// 根據 ID 獲取特定活動
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityResponse>> GetActivityById(int id)
        {
            try
            {
                _logger.LogInformation($"開始獲取活動 ID: {id}");

                var activity = await _context.Activities
                    .Include(a => a.Worker)
                    .FirstOrDefaultAsync(a => a.ActivityId == id);

                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {id}");
                    return NotFound(new { message = "找不到指定的活動" });
                }

                var response = new ActivityResponse
                {
                    ActivityId = activity.ActivityId,
                    ActivityName = activity.ActivityName ?? string.Empty,
                    Description = activity.Description,
                    ImageUrl = activity.ImageUrl,
                    Location = activity.Location ?? string.Empty,
                    MaxParticipants = activity.MaxParticipants ?? 0,
                    CurrentParticipants = activity.CurrentParticipants ?? 0,
                    StartDate = activity.StartDate,
                    EndDate = activity.EndDate,
                    SignupDeadline = activity.SignupDeadline,
                    WorkerId = activity.WorkerId ?? 0,
                    TargetAudience = activity.TargetAudience,
                    Category = activity.Category,
                    Status = activity.Status ?? string.Empty,
                    WorkerName = activity.Worker?.Name
                };

                _logger.LogInformation($"成功獲取活動 ID: {id}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取活動 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "獲取活動詳情失敗" });
            }
        }

        /// <summary>
        /// 建立新活動
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ActivityResponse>> CreateActivity([FromBody] CreateActivityRequest request)
        {
            try
            {
                _logger.LogInformation($"開始建立新活動，名稱: {request.ActivityName}");

                // 驗證分類
                if (!ActivityCategory.IsValidCategory(request.Category))
                {
                    return BadRequest(new { message = "無效的活動分類" });
                }

                var newActivity = new Activity
                {
                    ActivityName = request.ActivityName,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Location = request.Location,
                    MaxParticipants = request.MaxParticipants,
                    CurrentParticipants = 0,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    SignupDeadline = request.SignupDeadline,
                    WorkerId = request.WorkerId,
                    TargetAudience = request.TargetAudience,
                    Category = request.Category,
                    Status = "open"
                };

                _context.Activities.Add(newActivity);
                await _context.SaveChangesAsync();

                var createdActivity = await _context.Activities
                    .Include(a => a.Worker)
                    .FirstOrDefaultAsync(a => a.ActivityId == newActivity.ActivityId);

                var response = new ActivityResponse
                {
                    ActivityId = createdActivity!.ActivityId,
                    ActivityName = createdActivity.ActivityName ?? string.Empty,
                    Description = createdActivity.Description,
                    ImageUrl = createdActivity.ImageUrl,
                    Location = createdActivity.Location ?? string.Empty,
                    MaxParticipants = createdActivity.MaxParticipants ?? 0,
                    CurrentParticipants = createdActivity.CurrentParticipants ?? 0,
                    StartDate = createdActivity.StartDate,
                    EndDate = createdActivity.EndDate,
                    SignupDeadline = createdActivity.SignupDeadline,
                    WorkerId = createdActivity.WorkerId ?? 0,
                    TargetAudience = createdActivity.TargetAudience,
                    Category = createdActivity.Category,
                    Status = createdActivity.Status ?? string.Empty,
                    WorkerName = createdActivity.Worker?.Name
                };

                _logger.LogInformation($"成功建立活動 ID: {newActivity.ActivityId}");
                return CreatedAtAction(nameof(GetActivityById), new { id = newActivity.ActivityId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立活動時發生錯誤");
                return StatusCode(500, new { message = "建立活動失敗" });
            }
        }

        /// <summary>
        /// 更新活動資料
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateActivity(int id, [FromBody] UpdateActivityRequest request)
        {
            try
            {
                _logger.LogInformation($"開始更新活動 ID: {id}");

                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {id}");
                    return NotFound(new { message = "找不到指定的活動" });
                }

                if (request.ActivityName != null) activity.ActivityName = request.ActivityName;
                if (request.Description != null) activity.Description = request.Description;
                if (request.ImageUrl != null) activity.ImageUrl = request.ImageUrl;
                if (request.Location != null) activity.Location = request.Location;
                if (request.MaxParticipants.HasValue) activity.MaxParticipants = request.MaxParticipants.Value;
                if (request.CurrentParticipants.HasValue) activity.CurrentParticipants = request.CurrentParticipants.Value;
                if (request.StartDate.HasValue) activity.StartDate = request.StartDate.Value;
                if (request.EndDate.HasValue) activity.EndDate = request.EndDate.Value;
                if (request.SignupDeadline.HasValue) activity.SignupDeadline = request.SignupDeadline.Value;
                if (request.TargetAudience != null) activity.TargetAudience = request.TargetAudience;
                if (request.Category != null) 
                {
                    if (!ActivityCategory.IsValidCategory(request.Category))
                    {
                        return BadRequest(new { message = "無效的活動分類" });
                    }
                    activity.Category = request.Category;
                }
                if (request.Status != null) activity.Status = request.Status;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功更新活動 ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新活動 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "更新活動失敗" });
            }
        }

        /// <summary>
        /// 刪除活動
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteActivity(int id)
        {
            try
            {
                _logger.LogInformation($"開始刪除活動 ID: {id}");

                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {id}");
                    return NotFound(new { message = "找不到指定的活動" });
                }

                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功刪除活動 ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除活動 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "刪除活動失敗" });
            }
        }

        /// <summary>
        /// 分頁查詢活動列表
        /// </summary>
        [HttpGet("paged")]
        public async Task<ActionResult> GetPagedActivities(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? content = null,
            [FromQuery] string? status = null,
            [FromQuery] string? audience = null)
        {
            try
            {
                _logger.LogInformation($"查詢參數: page={page}, pageSize={pageSize}, status='{status}', audience='{audience}', content='{content}'");
                _logger.LogInformation($"參數檢查: status IsNullOrEmpty={string.IsNullOrEmpty(status)}, audience IsNullOrEmpty={string.IsNullOrEmpty(audience)}, content IsNullOrEmpty={string.IsNullOrEmpty(content)}");
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _context.Activities.Include(a => a.Worker).AsQueryable();

                // 應用搜尋條件
                if (!string.IsNullOrEmpty(status))
                {
                    _logger.LogInformation($"加入 status 條件: {status}");
                    query = query.Where(a => a.Status != null && a.Status.Trim().ToLower() == status.Trim().ToLower());
                }
                if (!string.IsNullOrEmpty(audience))
                {
                    _logger.LogInformation($"加入 audience 條件: {audience}");
                    query = query.Where(a => a.TargetAudience != null && a.TargetAudience.Trim().ToLower() == audience.Trim().ToLower());
                }
                if (!string.IsNullOrEmpty(content))
                {
                    _logger.LogInformation($"加入 content 條件: {content}");
                    query = query.Where(a =>
                        (a.ActivityName != null && a.ActivityName.ToLower().Contains(content.ToLower())) ||
                        (a.Location != null && a.Location.ToLower().Contains(content.ToLower())) ||
                        (a.Description != null && a.Description.ToLower().Contains(content.ToLower()))
                    );
                }

                // log SQL
                _logger.LogInformation($"查詢 SQL: {query.ToQueryString()}");

                // 排序
                query = query.OrderByDescending(a => a.StartDate);

                var total = await query.CountAsync();
                var activitiesData = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var activities = activitiesData.Select(a => new ActivityResponse
                {
                    ActivityId = a.ActivityId,
                    ActivityName = a.ActivityName ?? string.Empty,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    Location = a.Location ?? string.Empty,
                    MaxParticipants = a.MaxParticipants ?? 0,
                    CurrentParticipants = a.CurrentParticipants ?? 0,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    SignupDeadline = a.SignupDeadline,
                    WorkerId = a.WorkerId ?? 0,
                    TargetAudience = a.TargetAudience,
                    Category = a.Category,
                    Status = a.Status ?? string.Empty,
                    WorkerName = a.Worker?.Name
                }).ToList();

                return Ok(new {
                    data = activities,
                    total,
                    page,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分頁查詢活動時發生錯誤");
                return StatusCode(500, new { message = "分頁查詢活動失敗" });
            }
        }

        /// <summary>
        /// 測試圖片上傳端點（不實際上傳）
        /// </summary>
        [HttpPost("upload/test")]
        [AllowAnonymous]
        public Task<ActionResult<string>> TestUpload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Task.FromResult<ActionResult<string>>(BadRequest(new { message = "請選擇圖片檔案" }));
                }

                _logger.LogInformation($"收到測試上傳請求，檔案: {file.FileName}, 大小: {file.Length} bytes");

                return Task.FromResult<ActionResult<string>>(Ok(new { 
                    message = "測試上傳成功！",
                    fileName = file.FileName,
                    fileSize = file.Length,
                    contentType = file.ContentType,
                    imageUrl = "https://example.com/test-image.jpg"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試上傳失敗");
                return Task.FromResult<ActionResult<string>>(StatusCode(500, new { message = "測試上傳失敗", error = ex.Message }));
            }
        }

        /// <summary>
        /// 上傳圖片到 Azure Blob Storage
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

                // 暫時跳過 Azure，回傳假 URL 供測試
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var fakeUrl = $"https://fakeazure.blob.core.windows.net/activity-images/{fileName}";
                
                _logger.LogInformation($"圖片上傳模擬成功: {fileName}");
                
                // 模擬處理時間
                await Task.Delay(500);

                // 回傳假的 Azure URL
                return Ok(new { imageUrl = fakeUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "圖片上傳失敗");
                return StatusCode(500, new { message = "圖片上傳失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 取得所有活動分類選項
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        public ActionResult<List<CategoryOption>> GetCategories()
        {
            try
            {
                _logger.LogInformation("取得活動分類選項");
                var categories = ActivityCategory.GetAllCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得活動分類選項失敗");
                return StatusCode(500, new { message = "取得分類選項失敗" });
            }
        }

        /// <summary>
        /// 測試連接
        /// </summary>
        [HttpGet("test")]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("開始測試活動資料庫連接");

                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return BadRequest(new { 
                        message = "無法連接到資料庫", 
                        server = "ngosqlserver.database.windows.net",
                        database = "NGOPlatformDB",
                        status = "連接失敗" 
                    });
                }

                return Ok(new { 
                    message = "活動資料庫連接成功！", 
                    server = "ngosqlserver.database.windows.net",
                    database = "NGOPlatformDB",
                    status = "已連接" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "活動資料庫連接錯誤", 
                    error = ex.Message,
                    server = "ngosqlserver.database.windows.net",
                    database = "NGOPlatformDB",
                    status = "錯誤" 
                });
            }
        }
    }

    public class CreateActivityRequest
    {
        public string ActivityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignupDeadline { get; set; }
        public int WorkerId { get; set; }
        public string? TargetAudience { get; set; }
        public string? Category { get; set; }
    }

    public class UpdateActivityRequest
    {
        public string? ActivityName { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }
        public int? MaxParticipants { get; set; }
        public int? CurrentParticipants { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignupDeadline { get; set; }
        public string? TargetAudience { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
    }

    public class ActivityResponse
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignupDeadline { get; set; }
        public int WorkerId { get; set; }
        public string? TargetAudience { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? WorkerName { get; set; }
    }
} 