using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Data;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActivityController> _logger;

        public ActivityController(ApplicationDbContext context, ILogger<ActivityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 獲取所有活動
        /// </summary>
        /// <returns>活動列表</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityResponse>>> GetAllActivities()
        {
            try
            {
                _logger.LogInformation("開始獲取所有活動");

                var activities = await _context.Activities
                    .Include(a => a.Worker)
                    .Select(a => new ActivityResponse
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

                _logger.LogInformation($"成功獲取 {activities.Count} 個活動");
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取活動列表時發生錯誤");
                return StatusCode(500, new { message = "獲取活動列表失敗" });
            }
        }

        /// <summary>
        /// 根據ID獲取活動
        /// </summary>
        /// <param name="id">活動ID</param>
        /// <returns>活動詳情</returns>
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
        /// 搜尋活動
        /// </summary>
        /// <param name="query">搜尋關鍵字</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns>搜尋結果</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ActivityResponse>>> SearchActivities(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"開始搜尋活動，關鍵字: {query}, 頁碼: {page}, 每頁數量: {pageSize}");

                var queryable = _context.Activities
                    .Include(a => a.Worker)
                    .AsQueryable();

                // 如果有搜尋關鍵字，進行模糊搜尋
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryable = queryable.Where(a =>
                        a.ActivityName.Contains(query) ||
                        a.Description.Contains(query) ||
                        a.Location.Contains(query) ||
                        a.TargetAudience.Contains(query)
                    );
                }

                var totalCount = await queryable.CountAsync();
                var activities = await queryable
                    .OrderByDescending(a => a.StartDate ?? DateTime.MinValue)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ActivityResponse
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

                return Ok(new
                {
                    data = activities,
                    total = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
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
        /// </summary>
        /// <param name="request">建立活動請求</param>
        /// <returns>建立的活動</returns>
        [HttpPost]
        public async Task<ActionResult<ActivityResponse>> CreateActivity([FromBody] CreateActivityRequest request)
        {
            try
            {
                _logger.LogInformation($"開始建立新活動，名稱: {request.ActivityName}");

                // 建立新活動
                var newActivity = new Activity
                {
                    ActivityName = request.ActivityName,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Location = request.Location,
                    MaxParticipants = request.MaxParticipants,
                    CurrentParticipants = 0, // 新活動初始參與人數為0
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    SignupDeadline = request.SignupDeadline,
                    WorkerId = request.WorkerId,
                    TargetAudience = request.TargetAudience,
                    Status = "Active"
                };

                _context.Activities.Add(newActivity);
                await _context.SaveChangesAsync();

                // 重新查詢以獲取完整資料
                var createdActivity = await _context.Activities
                    .Include(a => a.Worker)
                    .FirstOrDefaultAsync(a => a.ActivityId == newActivity.ActivityId);

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
                return CreatedAtAction(nameof(GetActivityById), new { id = newActivity.ActivityId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立活動時發生錯誤");
                return StatusCode(500, new { message = "建立活動失敗" });
            }
        }

        /// <summary>
        /// 更新活動
        /// </summary>
        /// <param name="id">活動ID</param>
        /// <param name="request">更新活動請求</param>
        /// <returns>更新結果</returns>
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

                // 更新欄位
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
        /// <param name="id">活動ID</param>
        /// <returns>刪除結果</returns>
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
        /// 獲取活動統計資料
        /// </summary>
        /// <returns>統計資料</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<ActivityStatistics>> GetActivityStatistics()
        {
            try
            {
                _logger.LogInformation("開始獲取活動統計資料");

                var now = DateTime.Now;
                var totalActivities = await _context.Activities.CountAsync();
                var upcomingActivities = await _context.Activities
                    .Where(a => a.StartDate.HasValue && a.StartDate > now && a.Status == "Active")
                    .CountAsync();
                var ongoingActivities = await _context.Activities
                    .Where(a => a.StartDate.HasValue && a.EndDate.HasValue && 
                               a.StartDate <= now && a.EndDate >= now && a.Status == "Active")
                    .CountAsync();
                var completedActivities = await _context.Activities
                    .Where(a => a.EndDate.HasValue && a.EndDate < now)
                    .CountAsync();

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
        /// 測試端點 - 檢查資料庫連接
        /// </summary>
        /// <returns>測試結果</returns>
        [HttpGet("test")]
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

                // 測試查詢
                var activityCount = await _context.Activities.CountAsync();
                
                // 如果沒有活動資料，建立一些測試資料
                if (activityCount == 0)
                {
                    _logger.LogInformation("沒有活動資料，建立測試資料");
                    
                    var testActivities = new List<Activity>
                    {
                        new Activity
                        {
                            ActivityName = "青少年職涯探索工作坊",
                            Description = "幫助青少年了解不同職業，探索未來發展方向",
                            Location = "台北市信義區信義路五段7號",
                            MaxParticipants = 30,
                            CurrentParticipants = 25,
                            StartDate = DateTime.Now.AddDays(7),
                            EndDate = DateTime.Now.AddDays(7).AddHours(3),
                            SignupDeadline = DateTime.Now.AddDays(5),
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
                            StartDate = DateTime.Now.AddDays(-5),
                            EndDate = DateTime.Now.AddDays(-5).AddHours(2),
                            SignupDeadline = DateTime.Now.AddDays(-7),
                            WorkerId = 1,
                            TargetAudience = "志工",
                            Status = "Completed"
                        }
                    };

                    _context.Activities.AddRange(testActivities);
                    await _context.SaveChangesAsync();
                    
                    activityCount = await _context.Activities.CountAsync();
                    _logger.LogInformation($"已建立 {testActivities.Count} 個測試活動");
                }

                _logger.LogInformation($"活動資料庫連接成功，目前有 {activityCount} 個活動");

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

    // API 請求/回應模型
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
        public string Status { get; set; } = string.Empty;
        public string? WorkerName { get; set; }
    }

    public class ActivityStatistics
    {
        public int TotalActivities { get; set; }
        public int UpcomingActivities { get; set; }
        public int OngoingActivities { get; set; }
        public int CompletedActivities { get; set; }
    }
} 