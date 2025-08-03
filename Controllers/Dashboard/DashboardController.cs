using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;

namespace NGO_WebAPI_Backend.Controllers.Dashboard
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
        /// 獲取Dashboard基本資訊 (默認路由)
        /// </summary>
        [HttpGet]
        public ActionResult GetDashboard()
        {
            try
            {
                _logger.LogInformation("Dashboard API 運作正常");
                return Ok(new { 
                    message = "Dashboard API 運作正常", 
                    timestamp = DateTime.Now,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard API 發生錯誤");
                return StatusCode(500, new { message = "服務器錯誤" });
            }
        }

        /// <summary>
        /// 獲取Dashboard統計數據（支援WorkerId過濾）
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStats>> GetDashboardStats([FromQuery] int? workerId = null)
        {
            try
            {
                _logger.LogInformation("開始獲取Dashboard統計數據");

                var currentYear = DateTime.Now.Year;
                var lastYear = currentYear - 1;
                var startOfThisYear = new DateTime(currentYear, 1, 1);
                var startOfLastYear = new DateTime(lastYear, 1, 1);
                var startOfThisYearLastYear = new DateTime(lastYear, 1, 1);

                // 個案統計
                var totalCases = await _context.Cases.CountAsync();
                var thisYearCases = await _context.Cases
                    .Where(c => c.CreatedAt >= startOfThisYear)
                    .CountAsync();
                var lastYearCasesAtThisTime = await _context.Cases
                    .Where(c => c.CreatedAt >= startOfLastYear && c.CreatedAt < startOfThisYear)
                    .CountAsync();

                // 志工統計 (Workers表沒有CreateTime欄位，暫時使用簡化邏輯)
                var totalWorkers = await _context.Workers.CountAsync();
                var thisYearWorkers = 0; // Workers表沒有創建日期欄位，暫時設為0
                var lastYearWorkersAtThisTime = 1; // 避免除零錯誤

                var stats = new DashboardStats
                {
                    // 個案總數和今年新增數據
                    TotalCases = totalCases,
                    ThisYearNewCases = thisYearCases,
                    CasesGrowthPercentage = lastYearCasesAtThisTime > 0 
                        ? Math.Round((double)thisYearCases / lastYearCasesAtThisTime * 100 - 100, 1)
                        : (thisYearCases > 0 ? 100.0 : 0.0),
                    
                    // 志工總數和今年新增數據
                    TotalWorkers = totalWorkers,
                    ThisYearNewWorkers = thisYearWorkers,
                    WorkersGrowthPercentage = lastYearWorkersAtThisTime > 0 
                        ? Math.Round((double)thisYearWorkers / lastYearWorkersAtThisTime * 100 - 100, 1)
                        : (thisYearWorkers > 0 ? 100.0 : 0.0),
                    
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

                _logger.LogInformation($"統計數據獲取成功: 個案{stats.TotalCases}(今年新增{stats.ThisYearNewCases}), 志工{stats.TotalWorkers}(今年新增{stats.ThisYearNewWorkers}), 活動{stats.TotalActivities}, 本月完成{stats.MonthlyCompletedActivities}");
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
        /// 獲取個案縣市分佈數據 (用於地圖顯示)
        /// </summary>
        [HttpGet("county-distribution")]
        public async Task<ActionResult<List<CountyDistribution>>> GetCountyDistribution()
        {
            try
            {
                _logger.LogInformation("開始獲取個案縣市分佈數據");

                // 城市到縣市的對應字典
                var cityToCountyMap = new Dictionary<string, string>
                {
                    {"台北市", "台北市"}, {"新北市", "新北市"}, {"桃園市", "桃園市"}, {"台中市", "台中市"},
                    {"台南市", "台南市"}, {"高雄市", "高雄市"}, {"基隆市", "基隆市"}, {"新竹市", "新竹市"},
                    {"嘉義市", "嘉義市"}, {"新竹縣", "新竹縣"}, {"苗栗縣", "苗栗縣"}, {"彰化縣", "彰化縣"},
                    {"南投縣", "南投縣"}, {"雲林縣", "雲林縣"}, {"嘉義縣", "嘉義縣"}, {"屏東縣", "屏東縣"},
                    {"宜蘭縣", "宜蘭縣"}, {"花蓮縣", "花蓮縣"}, {"台東縣", "台東縣"}, {"澎湖縣", "澎湖縣"},
                    {"金門縣", "金門縣"}, {"連江縣", "連江縣"}
                };

                var caseStats = await _context.Cases
                    .GroupBy(c => c.City)
                    .Select(g => new 
                    {
                        City = g.Key ?? "未知",
                        Count = g.Count()
                    })
                    .ToListAsync();

                // 將城市資料轉換為縣市資料
                var countyStats = caseStats
                    .Where(c => cityToCountyMap.ContainsKey(c.City))
                    .GroupBy(c => cityToCountyMap[c.City])
                    .Select(g => new CountyDistribution
                    {
                        County = g.Key,
                        Count = g.Sum(x => x.Count)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // 如果有未對應的城市，也加入結果
                var unmappedCities = caseStats
                    .Where(c => !cityToCountyMap.ContainsKey(c.City) && c.City != "未知")
                    .Select(c => new CountyDistribution
                    {
                        County = c.City,
                        Count = c.Count
                    });

                countyStats.AddRange(unmappedCities);

                _logger.LogInformation($"個案縣市分佈數據獲取成功，共{countyStats.Count}個縣市");
                return Ok(countyStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個案縣市分佈數據時發生錯誤");
                return StatusCode(500, new { message = "獲取縣市分佈數據失敗" });
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
        public int ThisYearNewCases { get; set; }
        public double CasesGrowthPercentage { get; set; }
        
        public int TotalWorkers { get; set; }
        public int ThisYearNewWorkers { get; set; }
        public double WorkersGrowthPercentage { get; set; }
        
        public int TotalActivities { get; set; }
        public int MonthlyCompletedActivities { get; set; }
        
        // 為了向後相容保留 TotalUsers，但實際使用 TotalWorkers
        public int TotalUsers => TotalWorkers;
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

    public class CountyDistribution
    {
        public string County { get; set; } = string.Empty;
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