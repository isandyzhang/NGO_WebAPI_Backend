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
                    .Include(e => e.Worker)
                    .ToListAsync();

                var response = emergencyNeeds.Select(e => new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = e.EmergencyNeedId,
                    ItemName = e.ItemName ?? e.Supply?.SupplyName ?? "未知物品",
                    Category = e.Category ?? e.Supply?.SupplyCategory?.SupplyCategoryName ?? "未分類",
                    Quantity = e.Quantity ?? 0,
                    Unit = e.Unit ?? "個",
                    Urgency = e.Urgency ?? "medium",
                    RequestedBy = e.RequestedBy ?? e.Worker?.Name ?? "未知申請人",
                    RequestDate = e.RequestDate ?? DateTime.Now,
                    Status = e.Status ?? "pending",
                    EstimatedCost = e.EstimatedCost ?? 0,
                    CaseName = e.Case?.Name ?? "未知個案",
                    CaseId = e.CaseId?.ToString() ?? "未知",
                    Matched = e.Matched ?? false,
                    EmergencyReason = e.EmergencyReason ?? "未提供原因"
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

                var emergencyNeeds = await _context.EmergencySupplyNeeds.ToListAsync();

                var statistics = new EmergencySupplyNeedStatistics
                {
                    TotalRequests = emergencyNeeds.Count,
                    PendingRequests = emergencyNeeds.Count(e => e.Status == "pending"),
                    ApprovedRequests = emergencyNeeds.Count(e => e.Status == "approved"),
                    RejectedRequests = emergencyNeeds.Count(e => e.Status == "rejected"),
                    TotalEstimatedCost = emergencyNeeds.Sum(e => e.EstimatedCost ?? 0)
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
                    ItemName = emergencyNeed.ItemName ?? emergencyNeed.Supply?.SupplyName ?? "未知物品",
                    Category = emergencyNeed.Category ?? emergencyNeed.Supply?.SupplyCategory?.SupplyCategoryName ?? "未分類",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    Unit = emergencyNeed.Unit ?? "個",
                    Urgency = emergencyNeed.Urgency ?? "medium",
                    RequestedBy = emergencyNeed.RequestedBy ?? emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.RequestDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    EstimatedCost = emergencyNeed.EstimatedCost ?? 0,
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Matched ?? false,
                    EmergencyReason = emergencyNeed.EmergencyReason ?? "未提供原因"
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
                    ItemName = request.ItemName,
                    Category = request.Category,
                    Unit = request.Unit,
                    Urgency = request.Urgency ?? "medium",
                    RequestedBy = request.RequestedBy,
                    RequestDate = request.RequestDate ?? DateTime.Now,
                    EstimatedCost = request.EstimatedCost,
                    EmergencyReason = request.EmergencyReason,
                    Matched = false
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
                await _context.Entry(emergencyNeed)
                    .Reference(e => e.Worker)
                    .LoadAsync();

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = emergencyNeed.EmergencyNeedId,
                    ItemName = emergencyNeed.ItemName ?? emergencyNeed.Supply?.SupplyName ?? "未知物品",
                    Category = emergencyNeed.Category ?? emergencyNeed.Supply?.SupplyCategory?.SupplyCategoryName ?? "未分類",
                    Quantity = emergencyNeed.Quantity ?? 0,
                    Unit = emergencyNeed.Unit ?? "個",
                    Urgency = emergencyNeed.Urgency ?? "medium",
                    RequestedBy = emergencyNeed.RequestedBy ?? emergencyNeed.Worker?.Name ?? "未知申請人",
                    RequestDate = emergencyNeed.RequestDate ?? DateTime.Now,
                    Status = emergencyNeed.Status ?? "pending",
                    EstimatedCost = emergencyNeed.EstimatedCost ?? 0,
                    CaseName = emergencyNeed.Case?.Name ?? "未知個案",
                    CaseId = emergencyNeed.CaseId?.ToString() ?? "未知",
                    Matched = emergencyNeed.Matched ?? false,
                    EmergencyReason = emergencyNeed.EmergencyReason ?? "未提供原因"
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
        public string Urgency { get; set; } = string.Empty;
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
        public string? ItemName { get; set; }
        public string? Category { get; set; }
        public string? Unit { get; set; }
        public string? Urgency { get; set; }
        public string? RequestedBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? EmergencyReason { get; set; }
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