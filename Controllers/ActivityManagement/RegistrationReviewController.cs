using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NGO_WebAPI_Backend.Models.Infrastructure;

namespace NGO_WebAPI_Backend.Controllers.ActivityManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationReviewController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<RegistrationReviewController> _logger;

        public RegistrationReviewController(NgoplatformDbContext context, ILogger<RegistrationReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 查詢所有個案報名
        [HttpGet("case")]
        public async Task<ActionResult<IEnumerable<object>>> GetCaseRegistrations()
        {
            try
            {
                _logger.LogInformation("開始查詢個案報名資料");

                var registrations = await _context.CaseActivityRegistrations
                    .Join(_context.Cases, cr => cr.CaseId, c => c.CaseId, (cr, c) => new { cr, c })
                    .Join(_context.Activities, temp => temp.cr.ActivityId, a => a.ActivityId, (temp, a) => new
                    {
                        Id = temp.cr.RegistrationId,
                        CaseName = temp.c.Name ?? "未知個案",
                        ActivityName = a.ActivityName ?? "未知活動",
                        Status = temp.cr.Status ?? "registered"
                    })
                    .ToListAsync();

                _logger.LogInformation($"成功查詢到 {registrations.Count} 筆個案報名資料");
                
                return Ok(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢個案報名資料時發生錯誤");
                return StatusCode(500, new { message = "查詢個案報名資料失敗", error = ex.Message });
            }
        }

        // 查詢所有一般使用者報名
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserRegistrations()
        {
            try
            {
                _logger.LogInformation("開始查詢民眾報名資料");

                var registrations = await _context.UserActivityRegistrations
                    .Join(_context.Users, ur => ur.UserId, u => u.UserId, (ur, u) => new { ur, u })
                    .Join(_context.Activities, temp => temp.ur.ActivityId, a => a.ActivityId, (temp, a) => new
                    {
                        Id = temp.ur.RegistrationId,
                        UserId = temp.ur.UserId,
                        UserName = temp.u.Name ?? $"用戶{temp.ur.UserId}",
                        ActivityId = temp.ur.ActivityId,
                        ActivityName = a.ActivityName ?? $"活動{temp.ur.ActivityId}",
                        NumberOfCompanions = temp.ur.NumberOfCompanions ?? 0,
                        Status = temp.ur.Status ?? "registered"
                    })
                    .ToListAsync();

                _logger.LogInformation($"成功查詢到 {registrations.Count} 筆民眾報名資料");
                
                return Ok(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢民眾報名資料時發生錯誤");
                return StatusCode(500, new { message = "查詢民眾報名資料失敗", error = ex.Message });
            }
        }

        // 個案報名審核（同意/取消）
        [HttpPut("case/{id}/status")]
        public async Task<IActionResult> UpdateCaseRegistrationStatus(int id, [FromBody] UpdateStatusRequest req)
        {
            try
            {
                _logger.LogInformation($"開始更新個案報名狀態，ID: {id}, 新狀態: {req.Status}");

                var reg = await _context.CaseActivityRegistrations.FindAsync(id);
                if (reg == null)
                {
                    _logger.LogWarning($"找不到個案報名 ID: {id}");
                    return NotFound(new { message = "找不到指定的個案報名" });
                }

                var activity = await _context.Activities.FindAsync(reg.ActivityId);
                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {reg.ActivityId}");
                    return NotFound(new { message = "找不到相關的活動" });
                }

                string oldStatus = reg.Status ?? string.Empty;

                // 步驟1：更新報名狀態
                reg.Status = req.Status;
                await _context.SaveChangesAsync();

                // 步驟2：分別更新活動參與人數，避免觸發器衝突
                if (oldStatus == "Approved" && req.Status == "cancelled")
                {
                    // 從批准改為取消，減少參與人數
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Activities SET CurrentParticipants = CASE WHEN CurrentParticipants >= 1 THEN CurrentParticipants - 1 ELSE 0 END WHERE ActivityId = {0}",
                        reg.ActivityId ?? 0);
                }
                else if (oldStatus != "Approved" && req.Status == "Approved")
                {
                    // 從未批准改為批准，增加參與人數
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Activities SET CurrentParticipants = ISNULL(CurrentParticipants, 0) + 1 WHERE ActivityId = {0}",
                        reg.ActivityId ?? 0);
                }

                _logger.LogInformation($"成功更新個案報名狀態，ID: {id}");
                return Ok(new { message = "狀態更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新個案報名狀態時發生錯誤，ID: {id}");
                return StatusCode(500, new { message = "更新狀態失敗", error = ex.Message });
            }
        }

        // 一般使用者報名審核（同意/取消）
        [HttpPut("user/{id}/status")]
        public async Task<IActionResult> UpdateUserRegistrationStatus(int id, [FromBody] UpdateStatusRequest req)
        {
            try
            {
                _logger.LogInformation($"開始更新民眾報名狀態，ID: {id}, 新狀態: {req.Status}");

                var reg = await _context.UserActivityRegistrations.FindAsync(id);
                if (reg == null)
                {
                    _logger.LogWarning($"找不到民眾報名 ID: {id}");
                    return NotFound(new { message = "找不到指定的民眾報名" });
                }

                var activity = await _context.Activities.FindAsync(reg.ActivityId);
                if (activity == null)
                {
                    _logger.LogWarning($"找不到活動 ID: {reg.ActivityId}");
                    return NotFound(new { message = "找不到相關的活動" });
                }

                int delta = 1 + (reg.NumberOfCompanions ?? 0);
                string oldStatus = reg.Status ?? string.Empty;

                // 步驟1：更新報名狀態
                reg.Status = req.Status;
                await _context.SaveChangesAsync();

                // 步驟2：分別更新活動參與人數，避免觸發器衝突
                if (oldStatus == "Approved" && req.Status == "cancelled")
                {
                    // 從批准改為取消，減少參與人數
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Activities SET CurrentParticipants = CASE WHEN CurrentParticipants >= {0} THEN CurrentParticipants - {0} ELSE 0 END WHERE ActivityId = {1}",
                        delta, reg.ActivityId ?? 0);
                }
                else if (oldStatus != "Approved" && req.Status == "Approved")
                {
                    // 從未批准改為批准，增加參與人數
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Activities SET CurrentParticipants = ISNULL(CurrentParticipants, 0) + {0} WHERE ActivityId = {1}",
                        delta, reg.ActivityId ?? 0);
                }

                _logger.LogInformation($"成功更新民眾報名狀態，ID: {id}");
                return Ok(new { message = "狀態更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新民眾報名狀態時發生錯誤，ID: {id}");
                return StatusCode(500, new { message = "更新狀態失敗", error = ex.Message });
            }
        }


    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
} 