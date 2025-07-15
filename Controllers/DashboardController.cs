using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(NgoplatformDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 獲取Dashboard統計數據
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStats>> GetDashboardStats()
        {
            try
            {
                _logger.LogInformation("開始獲取Dashboard統計數據");

                var stats = new DashboardStats
                {
                    // 個案總數
                    TotalCases = await _context.Cases.CountAsync(),
                    
                    // 用戶總數
                    TotalUsers = await _context.Users.CountAsync(),
                    
                    // 活動總數
                    TotalActivities = await _context.Activities.CountAsync(),
                    
                    // 本月完成活動數
                    MonthlyCompletedActivities = await _context.Activities
                        .Where(a => a.Status == "completed" && 
                                   a.EndDate.HasValue &&
                                   a.EndDate.Value.Month == DateTime.Now.Month &&
                                   a.EndDate.Value.Year == DateTime.Now.Year)
                        .CountAsync()
                };

                _logger.LogInformation($"統計數據獲取成功: 個案{stats.TotalCases}, 用戶{stats.TotalUsers}, 活動{stats.TotalActivities}, 本月完成{stats.MonthlyCompletedActivities}");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取Dashboard統計數據時發生錯誤");
                return StatusCode(500, new { message = "獲取統計數據失敗" });
            }
        }

        /// <summary>
        /// 獲取性別分佈數據
        /// </summary>
        [HttpGet("gender-distribution")]
        public async Task<ActionResult<List<GenderDistribution>>> GetGenderDistribution()
        {
            try
            {
                _logger.LogInformation("開始獲取性別分佈數據");

                var genderStats = await _context.Cases
                    .GroupBy(c => c.Gender)
                    .Select(g => new GenderDistribution
                    {
                        Gender = g.Key ?? "未知",
                        Count = g.Count()
                    })
                    .ToListAsync();

                _logger.LogInformation($"性別分佈數據獲取成功，共{genderStats.Count}個分類");
                return Ok(genderStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取性別分佈數據時發生錯誤");
                return StatusCode(500, new { message = "獲取性別分佈數據失敗" });
            }
        }

        /// <summary>
        /// 獲取個案城市分佈數據
        /// </summary>
        [HttpGet("case-distribution")]
        public async Task<ActionResult<List<CaseDistribution>>> GetCaseDistribution()
        {
            try
            {
                _logger.LogInformation("開始獲取個案城市分佈數據");

                var caseStats = await _context.Cases
                    .GroupBy(c => c.City)
                    .Select(g => new CaseDistribution
                    {
                        City = g.Key ?? "未知",
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                _logger.LogInformation($"個案城市分佈數據獲取成功，共{caseStats.Count}個城市");
                return Ok(caseStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個案城市分佈數據時發生錯誤");
                return StatusCode(500, new { message = "獲取個案分佈數據失敗" });
            }
        }

        /// <summary>
        /// 獲取個案困難類型分析數據
        /// </summary>
        [HttpGet("difficulty-analysis")]
        public async Task<ActionResult<List<DifficultyAnalysis>>> GetDifficultyAnalysis()
        {
            try
            {
                _logger.LogInformation("開始獲取個案困難類型分析數據");

                var difficultyStats = await _context.Cases
                    .Where(c => c.Description != null)
                    .Select(c => new { Description = c.Description })
                    .ToListAsync();

                var groupedStats = difficultyStats
                    .GroupBy(c => c.Description ?? "未知")
                    .Select(g => new DifficultyAnalysis
                    {
                        DifficultyType = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                _logger.LogInformation($"個案困難類型分析數據獲取成功，共{groupedStats.Count}個類型");
                return Ok(groupedStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個案困難類型分析數據時發生錯誤");
                return StatusCode(500, new { message = "獲取困難分析數據失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 獲取用戶近期活動數據
        /// </summary>
        [HttpGet("recent-activities/{workerId}")]
        public async Task<ActionResult<List<RecentActivity>>> GetRecentActivities(int workerId)
        {
            try
            {
                _logger.LogInformation($"開始獲取用戶{workerId}的近期活動數據");

                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                
                var recentActivities = await _context.Schedules
                    .Where(s => s.WorkerId == workerId && s.StartTime >= thirtyDaysAgo)
                    .OrderByDescending(s => s.StartTime)
                    .Take(10)
                    .Select(s => new RecentActivity
                    {
                        ActivityId = s.ScheduleId,
                        ActivityName = s.EventName ?? "未知活動",
                        ActivityDate = s.StartTime ?? DateTime.Now,
                        Status = s.Status ?? "未知",
                        Location = s.Description ?? "未知地點"
                    })
                    .ToListAsync();

                _logger.LogInformation($"用戶{workerId}近期活動數據獲取成功，共{recentActivities.Count}筆");
                return Ok(recentActivities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取用戶{workerId}近期活動數據時發生錯誤");
                return StatusCode(500, new { message = "獲取近期活動數據失敗" });
            }
        }
    }

    // 數據模型
    public class DashboardStats
    {
        public int TotalCases { get; set; }
        public int TotalUsers { get; set; }
        public int TotalActivities { get; set; }
        public int MonthlyCompletedActivities { get; set; }
    }

    public class GenderDistribution
    {
        public string Gender { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CaseDistribution
    {
        public string City { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DifficultyAnalysis
    {
        public string DifficultyType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecentActivity
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
} 