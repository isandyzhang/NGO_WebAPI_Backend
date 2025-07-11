using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationReviewController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<RegistrationReviewController> _logger;

        public RegistrationReviewController(MyDbContext context, ILogger<RegistrationReviewController> logger)
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
                    .Include(r => r.Case)
                    .Include(r => r.Activity)
                    .Select(r => new
                    {
                        Id = r.RegistrationId,
                        CaseName = r.Case != null ? r.Case.Name : "未知個案",
                        ActivityName = r.Activity != null ? r.Activity.ActivityName : "未知活動",
                        Status = r.Status
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

                // 直接查詢 UserActivityRegistrations 表，不關聯其他表
                var registrations = await _context.UserActivityRegistrations
                    .Select(r => new
                    {
                        Id = r.RegistrationId,
                        UserId = r.UserId,
                        UserName = $"用戶{r.UserId}", // 暫時使用 UserId 作為顯示名稱
                        ActivityId = r.ActivityId,
                        ActivityName = $"活動{r.ActivityId}", // 暫時使用 ActivityId 作為顯示名稱
                        NumberOfCompanions = r.NumberOfCompanions ?? 0,
                        Status = r.Status
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

                // 更新參與人數
                if (reg.Status == "Approved" && req.Status == "Cancelled")
                    activity.CurrentParticipants = Math.Max(0, (activity.CurrentParticipants ?? 0) - 1);
                if (reg.Status != "Approved" && req.Status == "Approved")
                    activity.CurrentParticipants = (activity.CurrentParticipants ?? 0) + 1;

                reg.Status = req.Status;
                await _context.SaveChangesAsync();

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

                // 更新參與人數
                if (reg.Status == "Approved" && req.Status == "Cancelled")
                    activity.CurrentParticipants = Math.Max(0, (activity.CurrentParticipants ?? 0) - delta);
                if (reg.Status != "Approved" && req.Status == "Approved")
                    activity.CurrentParticipants = (activity.CurrentParticipants ?? 0) + delta;

                reg.Status = req.Status;
                await _context.SaveChangesAsync();

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