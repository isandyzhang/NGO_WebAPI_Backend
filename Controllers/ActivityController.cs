using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Data;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    /// <summary>
    /// 活動管理控制器
    /// 
    /// 這個控制器負責處理所有與活動相關的 HTTP 請求：
    /// - 獲取活動列表
    /// - 建立新活動
    /// - 更新活動資料
    /// - 刪除活動
    /// - 搜尋活動
    /// - 獲取統計資料
    /// </summary>
    [ApiController]  // 標記這是一個 API 控制器
    [Route("api/[controller]")]  // 路由：api/activity
    public class ActivityController : ControllerBase
    {
        // 資料庫上下文 - 用來存取資料庫
        private readonly ApplicationDbContext _context;
        
        // 日誌記錄器 - 用來記錄系統日誌
        private readonly ILogger<ActivityController> _logger;

        /// <summary>
        /// 建構函式 - 依賴注入
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        public ActivityController(ApplicationDbContext context, ILogger<ActivityController> logger)
        {
            _context = context;  // 注入資料庫上下文
            _logger = logger;    // 注入日誌記錄器
        }

        /// <summary>
        /// 獲取所有活動列表
        /// HTTP GET: /api/activity
        /// </summary>
        /// <returns>所有活動的列表</returns>
        [HttpGet]  // 處理 GET 請求
        public async Task<ActionResult<IEnumerable<ActivityResponse>>> GetAllActivities()
        {
            try
            {
                _logger.LogInformation("開始獲取所有活動");

                // 查詢資料庫，包含關聯的 Worker 資料
                var activities = await _context.Activities
                    .Include(a => a.Worker)  // 包含工作人員資料
                    .Select(a => new ActivityResponse  // 轉換為回應格式
                    {
                        ActivityId = a.ActivityId,
                        ActivityName = a.ActivityName,
                        Description = a.Description,
                        ImageUrl = a.ImageUrl,
                        Location = a.Location,
                        MaxParticipants = a.MaxParticipants,
                        CurrentParticipants = a.CurrentParticipants,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        SignupDeadline = a.SignupDeadline,
                        WorkerId = a.WorkerId,
                        TargetAudience = a.TargetAudience,
                        Status = a.Status,
                        WorkerName = a.Worker != null ? a.Worker.Name : null  // 安全存取工作人員姓名
                    })
                    .ToListAsync();  // 非同步執行查詢

                _logger.LogInformation($"成功獲取 {activities.Count} 個活動");
                return Ok(activities);  // 回傳 200 OK 狀態碼
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取活動列表時發生錯誤");
                return StatusCode(500, new { message = "獲取活動列表失敗" });  // 回傳 500 錯誤
            }
        }

        /// <summary>
        /// 根據 ID 獲取特定活動
        /// HTTP GET: /api/activity/{id}
        /// </summary>
        /// <param name="id">活動 ID</param>
        /// <returns>特定活動的詳細資料</returns>
        [HttpGet("{id}")]  // 處理 GET 請求，{id} 是路由參數
        public async Task<ActionResult<ActivityResponse>> GetActivityById(int id)
        {
            try
            {
                _logger.LogInformation($"開始獲取活動 ID: {id}");

                // 根據 ID 查詢活動，包含工作人員資料
                var activity = await _context.Activities
                    .Include(a => a.Worker)
                    .FirstOrDefaultAsync(a => a.ActivityId == id);  // 找到第一個符合條件的活動

                // 如果找不到活動，回傳 404 Not Found
                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {id}");
                    return NotFound(new { message = "找不到指定的活動" });
                }

                // 轉換為回應格式
                var response = new ActivityResponse
                {
                    ActivityId = activity.ActivityId,
                    ActivityName = activity.ActivityName,
                    Description = activity.Description,
                    ImageUrl = activity.ImageUrl,
                    Location = activity.Location,
                    MaxParticipants = activity.MaxParticipants,
                    CurrentParticipants = activity.CurrentParticipants,
                    StartDate = activity.StartDate,
                    EndDate = activity.EndDate,
                    SignupDeadline = activity.SignupDeadline,
                    WorkerId = activity.WorkerId,
                    TargetAudience = activity.TargetAudience,
                    Status = activity.Status,
                    WorkerName = activity.Worker?.Name  // 使用 null 條件運算子
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
        /// 搜尋活動（支援分頁）
        /// HTTP GET: /api/activity/search?query=關鍵字&page=1&pageSize=10
        /// </summary>
        /// <param name="query">搜尋關鍵字</param>
        /// <param name="page">頁碼（預設 1）</param>
        /// <param name="pageSize">每頁數量（預設 10）</param>
        /// <returns>搜尋結果和分頁資訊</returns>
        [HttpGet("search")]  // 處理 GET 請求：/api/activity/search
        public async Task<ActionResult<IEnumerable<ActivityResponse>>> SearchActivities(
            [FromQuery] string? query,  // 從 URL 查詢參數取得
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"開始搜尋活動，關鍵字: {query}, 頁碼: {page}, 每頁數量: {pageSize}");

                // 建立查詢基礎
                var queryable = _context.Activities
                    .Include(a => a.Worker)
                    .AsQueryable();  // 轉換為可查詢物件

                // 如果有搜尋關鍵字，進行模糊搜尋
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryable = queryable.Where(a =>
                        a.ActivityName.Contains(query) ||      // 活動名稱包含關鍵字
                        a.Description.Contains(query) ||       // 描述包含關鍵字
                        a.Location.Contains(query) ||          // 地點包含關鍵字
                        a.TargetAudience.Contains(query)       // 目標對象包含關鍵字
                    );
                }

                // 計算總數量
                var totalCount = await queryable.CountAsync();
                
                // 執行分頁查詢
                var activities = await queryable
                    .OrderByDescending(a => a.StartDate ?? DateTime.MinValue)  // 按開始日期降序排列
                    .Skip((page - 1) * pageSize)  // 跳過前面的頁面
                    .Take(pageSize)  // 只取當前頁面的資料
                    .Select(a => new ActivityResponse  // 轉換為回應格式
                    {
                        ActivityId = a.ActivityId,
                        ActivityName = a.ActivityName,
                        Description = a.Description,
                        ImageUrl = a.ImageUrl,
                        Location = a.Location,
                        MaxParticipants = a.MaxParticipants,
                        CurrentParticipants = a.CurrentParticipants,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        SignupDeadline = a.SignupDeadline,
                        WorkerId = a.WorkerId,
                        TargetAudience = a.TargetAudience,
                        Status = a.Status,
                        WorkerName = a.Worker != null ? a.Worker.Name : null
                    })
                    .ToListAsync();

                _logger.LogInformation($"搜尋完成，找到 {activities.Count} 個活動，總計 {totalCount} 個");

                // 回傳包含分頁資訊的結果
                return Ok(new
                {
                    data = activities,  // 活動資料
                    total = totalCount,  // 總數量
                    page = page,  // 當前頁碼
                    pageSize = pageSize,  // 每頁數量
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)  // 總頁數
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋活動時發生錯誤");
                return StatusCode(500, new { message = "搜尋活動失敗" });
            }
        }

        /// <summary>
        /// 建立新活動
        /// HTTP POST: /api/activity
        /// </summary>
        /// <param name="request">建立活動的請求資料</param>
        /// <returns>新建立的活動資料</returns>
        [HttpPost]  // 處理 POST 請求
        public async Task<ActionResult<ActivityResponse>> CreateActivity([FromBody] CreateActivityRequest request)
        {
            try
            {
                _logger.LogInformation($"開始建立新活動，名稱: {request.ActivityName}");

                // 建立新的活動實體
                var newActivity = new Activity
                {
                    ActivityName = request.ActivityName,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Location = request.Location,
                    MaxParticipants = request.MaxParticipants,
                    CurrentParticipants = 0,  // 新活動初始參與人數為 0
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    SignupDeadline = request.SignupDeadline,
                    WorkerId = request.WorkerId,
                    TargetAudience = request.TargetAudience,
                    Status = "Active"  // 新活動預設為啟用狀態
                };

                // 將新活動加入資料庫
                _context.Activities.Add(newActivity);
                await _context.SaveChangesAsync();  // 儲存變更

                // 重新查詢以獲取完整資料（包含工作人員資訊）
                var createdActivity = await _context.Activities
                    .Include(a => a.Worker)
                    .FirstOrDefaultAsync(a => a.ActivityId == newActivity.ActivityId);

                // 轉換為回應格式
                var response = new ActivityResponse
                {
                    ActivityId = createdActivity!.ActivityId,
                    ActivityName = createdActivity.ActivityName,
                    Description = createdActivity.Description,
                    ImageUrl = createdActivity.ImageUrl,
                    Location = createdActivity.Location,
                    MaxParticipants = createdActivity.MaxParticipants,
                    CurrentParticipants = createdActivity.CurrentParticipants,
                    StartDate = createdActivity.StartDate,
                    EndDate = createdActivity.EndDate,
                    SignupDeadline = createdActivity.SignupDeadline,
                    WorkerId = createdActivity.WorkerId,
                    TargetAudience = createdActivity.TargetAudience,
                    Status = createdActivity.Status,
                    WorkerName = createdActivity.Worker?.Name
                };

                _logger.LogInformation($"成功建立活動 ID: {newActivity.ActivityId}");
                
                // 回傳 201 Created 狀態碼，並提供新資源的位置
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
        /// HTTP PUT: /api/activity/{id}
        /// </summary>
        /// <param name="id">要更新的活動 ID</param>
        /// <param name="request">更新的資料</param>
        /// <returns>更新結果</returns>
        [HttpPut("{id}")]  // 處理 PUT 請求
        public async Task<ActionResult> UpdateActivity(int id, [FromBody] UpdateActivityRequest request)
        {
            try
            {
                _logger.LogInformation($"開始更新活動 ID: {id}");

                // 根據 ID 查找活動
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {id}");
                    return NotFound(new { message = "找不到指定的活動" });
                }

                // 只更新有提供的欄位（部分更新）
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
                if (request.Status != null) activity.Status = request.Status;

                // 儲存變更
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功更新活動 ID: {id}");
                return NoContent();  // 回傳 204 No Content（更新成功但沒有內容回傳）
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新活動 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "更新活動失敗" });
            }
        }

        /// <summary>
        /// 刪除活動
        /// HTTP DELETE: /api/activity/{id}
        /// </summary>
        /// <param name="id">要刪除的活動 ID</param>
        /// <returns>刪除結果</returns>
        [HttpDelete("{id}")]  // 處理 DELETE 請求
        public async Task<ActionResult> DeleteActivity(int id)
        {
            try
            {
                _logger.LogInformation($"開始刪除活動 ID: {id}");

                // 根據 ID 查找活動
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {id}");
                    return NotFound(new { message = "找不到指定的活動" });
                }

                // 從資料庫中移除活動
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功刪除活動 ID: {id}");
                return NoContent();  // 回傳 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除活動 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "刪除活動失敗" });
            }
        }

        /// <summary>
        /// 獲取活動統計資料
        /// HTTP GET: /api/activity/statistics
        /// </summary>
        /// <returns>活動統計資料</returns>
        [HttpGet("statistics")]  // 處理 GET 請求：/api/activity/statistics
        public async Task<ActionResult<ActivityStatistics>> GetActivityStatistics()
        {
            try
            {
                _logger.LogInformation("開始獲取活動統計資料");

                var now = DateTime.Now;  // 當前時間
                
                // 統計各種狀態的活動數量
                var totalActivities = await _context.Activities.CountAsync();  // 總活動數
                
                var upcomingActivities = await _context.Activities
                    .Where(a => a.StartDate.HasValue && a.StartDate > now && a.Status == "Active")  // 即將開始的活動
                    .CountAsync();
                    
                var ongoingActivities = await _context.Activities
                    .Where(a => a.StartDate.HasValue && a.EndDate.HasValue && 
                               a.StartDate <= now && a.EndDate >= now && a.Status == "Active")  // 正在進行的活動
                    .CountAsync();
                    
                var completedActivities = await _context.Activities
                    .Where(a => a.EndDate.HasValue && a.EndDate < now)  // 已完成的活動
                    .CountAsync();

                // 建立統計資料物件
                var statistics = new ActivityStatistics
                {
                    TotalActivities = totalActivities,
                    UpcomingActivities = upcomingActivities,
                    OngoingActivities = ongoingActivities,
                    CompletedActivities = completedActivities
                };

                _logger.LogInformation($"成功獲取活動統計資料: 總計 {totalActivities} 個活動");
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取活動統計資料時發生錯誤");
                return StatusCode(500, new { message = "獲取活動統計資料失敗" });
            }
        }

        /// <summary>
        /// 測試端點 - 檢查資料庫連接和建立測試資料
        /// HTTP GET: /api/activity/test
        /// </summary>
        /// <returns>測試結果</returns>
        [HttpGet("test")]  // 處理 GET 請求：/api/activity/test
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("開始測試活動資料庫連接");

                // 測試資料庫連接
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("無法連接到活動資料庫");
                    return StatusCode(500, new { message = "資料庫連接失敗" });
                }

                // 測試查詢 - 計算活動數量
                var activityCount = await _context.Activities.CountAsync();
                
                // 如果沒有活動資料，建立一些測試資料
                if (activityCount == 0)
                {
                    _logger.LogInformation("沒有活動資料，建立測試資料");
                    
                    // 建立測試活動列表
                    var testActivities = new List<Activity>
                    {
                        new Activity
                        {
                            ActivityName = "青少年職涯探索工作坊",
                            Description = "幫助青少年了解不同職業，探索未來發展方向",
                            Location = "台北市信義區信義路五段7號",
                            MaxParticipants = 30,
                            CurrentParticipants = 25,
                            StartDate = DateTime.Now.AddDays(7),  // 7 天後開始
                            EndDate = DateTime.Now.AddDays(7).AddHours(3),  // 持續 3 小時
                            SignupDeadline = DateTime.Now.AddDays(5),  // 5 天後截止報名
                            WorkerId = 1,
                            TargetAudience = "青少年",
                            Status = "Active"
                        },
                        new Activity
                        {
                            ActivityName = "長者數位學習課程",
                            Description = "教導長者使用智慧型手機和電腦",
                            Location = "新北市板橋區文化路一段100號",
                            MaxParticipants = 20,
                            CurrentParticipants = 18,
                            StartDate = DateTime.Now.AddDays(3),
                            EndDate = DateTime.Now.AddDays(3).AddHours(2),
                            SignupDeadline = DateTime.Now.AddDays(1),
                            WorkerId = 1,
                            TargetAudience = "長者",
                            Status = "Active"
                        },
                        new Activity
                        {
                            ActivityName = "環保淨灘活動",
                            Description = "清理海灘垃圾，保護海洋環境",
                            Location = "基隆市中山區中山一路1號",
                            MaxParticipants = 50,
                            CurrentParticipants = 35,
                            StartDate = DateTime.Now.AddDays(14),
                            EndDate = DateTime.Now.AddDays(14).AddHours(4),
                            SignupDeadline = DateTime.Now.AddDays(10),
                            WorkerId = 1,
                            TargetAudience = "一般民眾",
                            Status = "Active"
                        },
                        new Activity
                        {
                            ActivityName = "社區關懷訪問",
                            Description = "定期訪問社區中的獨居長者",
                            Location = "桃園市中壢區中正路100號",
                            MaxParticipants = 15,
                            CurrentParticipants = 12,
                            StartDate = DateTime.Now.AddDays(-5),  // 5 天前開始
                            EndDate = DateTime.Now.AddDays(-5).AddHours(2),
                            SignupDeadline = DateTime.Now.AddDays(-7),  // 7 天前截止
                            WorkerId = 1,
                            TargetAudience = "志工",
                            Status = "Completed"  // 已完成
                        }
                    };

                    // 將測試資料加入資料庫
                    _context.Activities.AddRange(testActivities);
                    await _context.SaveChangesAsync();
                    
                    activityCount = await _context.Activities.CountAsync();
                    _logger.LogInformation($"已建立 {testActivities.Count} 個測試活動");
                }

                _logger.LogInformation($"活動資料庫連接成功，目前有 {activityCount} 個活動");

                // 回傳測試結果
                return Ok(new
                {
                    message = "活動資料庫連接成功",
                    activityCount = activityCount,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試活動資料庫連接時發生錯誤");
                return StatusCode(500, new { message = "活動資料庫連接測試失敗", error = ex.Message });
            }
        }
    }

    // ==================== API 請求/回應模型 ====================

    /// <summary>
    /// 建立活動的請求模型
    /// 定義前端傳送建立活動時需要的資料格式
    /// </summary>
    public class CreateActivityRequest
    {
        public string ActivityName { get; set; } = string.Empty;  // 活動名稱（必填）
        public string? Description { get; set; }                  // 活動描述（可選）
        public string? ImageUrl { get; set; }                     // 活動圖片 URL（可選）
        public string Location { get; set; } = string.Empty;      // 活動地點（必填）
        public int MaxParticipants { get; set; }                  // 最大參與人數
        public DateTime? StartDate { get; set; }                  // 開始時間（可選）
        public DateTime? EndDate { get; set; }                    // 結束時間（可選）
        public DateTime? SignupDeadline { get; set; }             // 報名截止時間（可選）
        public int WorkerId { get; set; }                         // 負責工作人員 ID
        public string? TargetAudience { get; set; }               // 目標對象（可選）
    }

    /// <summary>
    /// 更新活動的請求模型
    /// 所有欄位都是可選的，只更新有提供的欄位
    /// </summary>
    public class UpdateActivityRequest
    {
        public string? ActivityName { get; set; }                 // 活動名稱
        public string? Description { get; set; }                  // 活動描述
        public string? ImageUrl { get; set; }                     // 活動圖片 URL
        public string? Location { get; set; }                     // 活動地點
        public int? MaxParticipants { get; set; }                 // 最大參與人數
        public int? CurrentParticipants { get; set; }             // 目前參與人數
        public DateTime? StartDate { get; set; }                  // 開始時間
        public DateTime? EndDate { get; set; }                    // 結束時間
        public DateTime? SignupDeadline { get; set; }             // 報名截止時間
        public string? TargetAudience { get; set; }               // 目標對象
        public string? Status { get; set; }                       // 活動狀態
    }

    /// <summary>
    /// 活動回應模型
    /// 定義回傳給前端的活動資料格式
    /// </summary>
    public class ActivityResponse
    {
        public int ActivityId { get; set; }                       // 活動 ID
        public string ActivityName { get; set; } = string.Empty;  // 活動名稱
        public string? Description { get; set; }                  // 活動描述
        public string? ImageUrl { get; set; }                     // 活動圖片 URL
        public string Location { get; set; } = string.Empty;      // 活動地點
        public int MaxParticipants { get; set; }                  // 最大參與人數
        public int CurrentParticipants { get; set; }              // 目前參與人數
        public DateTime? StartDate { get; set; }                  // 開始時間
        public DateTime? EndDate { get; set; }                    // 結束時間
        public DateTime? SignupDeadline { get; set; }             // 報名截止時間
        public int WorkerId { get; set; }                         // 負責工作人員 ID
        public string? TargetAudience { get; set; }               // 目標對象
        public string Status { get; set; } = string.Empty;        // 活動狀態
        public string? WorkerName { get; set; }                   // 工作人員姓名（關聯查詢）
    }

    /// <summary>
    /// 活動統計資料模型
    /// 用於回傳活動的統計資訊
    /// </summary>
    public class ActivityStatistics
    {
        public int TotalActivities { get; set; }      // 總活動數
        public int UpcomingActivities { get; set; }   // 即將開始的活動數
        public int OngoingActivities { get; set; }    // 正在進行的活動數
        public int CompletedActivities { get; set; }  // 已完成的活動數
    }
} 