using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencySupplyNeedController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<EmergencySupplyNeedController> _logger;

        public EmergencySupplyNeedController(NgoplatformDbContext context, ILogger<EmergencySupplyNeedController> logger)
        {
            _context = context;
            _logger = logger;
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

                var emergencyNeeds = await _context.EmergencySupplyNeeds
                    .Include(e => e.Case)
                    .Include(e => e.Supply)
                    .ThenInclude(s => s!.SupplyCategory)
                    .Include(e => e.Worker)
                    .ToListAsync();

                var response = emergencyNeeds.Select(e => new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = e.EmergencyNeedId,
                    ItemName = e.Supply?.SupplyName ?? "未知物品",
                    Category = e.Supply?.SupplyCategory?.SupplyCategoryName ?? "未分類",
                    Quantity = e.Quantity ?? 0,
                    Unit = "個",
                    RequestedBy = e.Worker?.Name ?? "未知申請人",
                    RequestDate = e.VisitDate ?? DateTime.Now,
                    Status = e.Status ?? "pending",
                    EstimatedCost = (e.Quantity ?? 0) * (e.Supply?.SupplyPrice ?? 0),
                    CaseName = e.Case?.Name ?? "未知個案",
                    CaseId = e.CaseId?.ToString() ?? "未知",
                    Matched = e.Status == "approved",
                    EmergencyReason = "緊急物資需求"
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

                var emergencyNeeds = await _context.EmergencySupplyNeeds
                    .Include(e => e.Supply)
                    .ToListAsync();

                var statistics = new EmergencySupplyNeedStatistics
                {
                    TotalRequests = emergencyNeeds.Count,
                    PendingRequests = emergencyNeeds.Count(e => e.Status == "pending"),
                    ApprovedRequests = emergencyNeeds.Count(e => e.Status == "approved"),
                    RejectedRequests = emergencyNeeds.Count(e => e.Status == "rejected"),
                    TotalEstimatedCost = emergencyNeeds.Sum(e => (e.Quantity ?? 0) * (e.Supply?.SupplyPrice ?? 0))
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
                    .Include(e => e.Supply)
                    .ThenInclude(s => s!.SupplyCategory)
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
                    ItemName = emergencyNeed.Supply?.SupplyName ?? "未知物品",
                    Category = emergencyNeed.Supply?.SupplyCategory?.SupplyCategoryName ?? "未分類",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    Unit = "個",
                    RequestedBy = emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.VisitDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    EstimatedCost = (emergencyNeed.Quantity ?? 0) * (emergencyNeed.Supply?.SupplyPrice ?? 0),
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Status == "approved",
                    EmergencyReason = "緊急物資需求"
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
                    SupplyId = request.SupplyId,
                    WorkerId = request.WorkerId,
                    Quantity = request.Quantity,
                    Status = request.Status ?? "pending",
                    VisitDate = request.RequestDate ?? DateTime.Now
                };

                _context.EmergencySupplyNeeds.Add(emergencyNeed);
                await _context.SaveChangesAsync();

                // 重新載入關聯資料
                await _context.Entry(emergencyNeed)
                    .Reference(e => e.Case)
                    .LoadAsync();
                await _context.Entry(emergencyNeed)
                    .Reference(e => e.Supply)
                    .LoadAsync();
                if (emergencyNeed.Supply != null)
                {
                    await _context.Entry(emergencyNeed.Supply)
                        .Reference(s => s.SupplyCategory)
                        .LoadAsync();
                }
                await _context.Entry(emergencyNeed)
                    .Reference(e => e.Worker)
                    .LoadAsync();

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = emergencyNeed.EmergencyNeedId,
                    ItemName = emergencyNeed.Supply?.SupplyName ?? "未知物品",
                    Category = emergencyNeed.Supply?.SupplyCategory?.SupplyCategoryName ?? "未分類",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    Unit = "個",
                    RequestedBy = emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.VisitDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    EstimatedCost = (emergencyNeed.Quantity ?? 0) * (emergencyNeed.Supply?.SupplyPrice ?? 0),
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Status == "approved",
                    EmergencyReason = "緊急物資需求"
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

                // 先獲取一些現有的Supply、Case、Worker數據
                var supplies = await _context.Supplies.Take(3).ToListAsync();
                var cases = await _context.Cases.Take(3).ToListAsync();
                var workers = await _context.Workers.Take(3).ToListAsync();

                if (!supplies.Any() || !cases.Any() || !workers.Any())
                {
                    return BadRequest(new { message = "缺少必要的基礎數據（Supply、Case、Worker）" });
                }

                var testData = new List<EmergencySupplyNeed>
                {
                    new EmergencySupplyNeed
                    {
                        CaseId = cases[0].CaseId,
                        SupplyId = supplies[0].SupplyId,
                        WorkerId = workers[0].WorkerId,
                        Quantity = 5,
                        Status = "pending",
                        VisitDate = DateTime.Now.AddDays(-2)
                    },
                    new EmergencySupplyNeed
                    {
                        CaseId = cases.Count > 1 ? cases[1].CaseId : cases[0].CaseId,
                        SupplyId = supplies.Count > 1 ? supplies[1].SupplyId : supplies[0].SupplyId,
                        WorkerId = workers.Count > 1 ? workers[1].WorkerId : workers[0].WorkerId,
                        Quantity = 3,
                        Status = "approved",
                        VisitDate = DateTime.Now.AddDays(-1)
                    },
                    new EmergencySupplyNeed
                    {
                        CaseId = cases.Count > 2 ? cases[2].CaseId : cases[0].CaseId,
                        SupplyId = supplies.Count > 2 ? supplies[2].SupplyId : supplies[0].SupplyId,
                        WorkerId = workers.Count > 2 ? workers[2].WorkerId : workers[0].WorkerId,
                        Quantity = 10,
                        Status = "pending",
                        VisitDate = DateTime.Now.AddHours(-6)
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
        public string Unit { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public string CaseName { get; set; } = string.Empty;
        public string CaseId { get; set; } = string.Empty;
        public bool Matched { get; set; }
        public string EmergencyReason { get; set; } = string.Empty;
    }

    public class CreateEmergencySupplyNeedRequest
    {
        public int? CaseId { get; set; }
        public int? SupplyId { get; set; }
        public int? WorkerId { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
        public DateTime? RequestDate { get; set; }
    }

    public class EmergencySupplyNeedStatistics
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public decimal TotalEstimatedCost { get; set; }
    }
} 