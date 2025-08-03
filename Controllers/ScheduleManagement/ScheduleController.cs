using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using System.Linq;

namespace NGO_WebAPI_Backend.Controllers.ScheduleManagement
{
    /// <summary>
    /// 行事曆管理控制器
    /// 
    /// 處理所有與活動排程（Schedule）相關的 HTTP 請求：
    /// - 依照 WorkerId 載入活動
    /// - 建立新活動
    /// - 更新活動
    /// - 刪除活動
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(NgoplatformDbContext context, ILogger<ScheduleController> logger)
        {
            _context = context;
            _logger = logger;
        }


        /// <summary>
        /// 搜尋指定員工 ID 的所有排程活動
        /// GET: /api/schedule/select/{workid}
        /// </summary>
        [HttpGet("select/{workid}")]
        public async Task<ActionResult<IEnumerable<object>>> GetSchedulesByWorkerId(int workid)
        {
            var schedules = await _context.Schedules
                .Where(s => s.WorkerId == workid)
                .OrderByDescending(s => s.StartTime)
                .Select(s => new
                {
                    s.ScheduleId,
                    s.WorkerId,
                    s.EventType,
                    s.EventName,
                    s.Description,
                    s.StartTime,
                    s.EndTime
                })
                .ToListAsync();

            if (!schedules.Any())
            {
                return NotFound("查無資料");
            }

            return Ok(schedules);
        }

        /// <summary>
        /// 取得所有排程活動（管理員權限）
        /// GET: /api/schedule
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllSchedules()
        {
            var schedules = await _context.Schedules
                .OrderByDescending(s => s.StartTime)
                .Select(s => new
                {
                    s.ScheduleId,
                    s.WorkerId,
                    s.EventType,
                    s.EventName,
                    s.Description,
                    s.StartTime,
                    s.EndTime
                })
                .ToListAsync();

            return Ok(schedules);
        }


        /// <summary>
        /// 建立新的行事曆活動
        /// HTTP POST: /api/schedule
        /// </summary>
        /// <param name="schedule">活動資料</param>
        /// <returns>建立後的活動資訊</returns>
        [HttpPost]
        public async Task<ActionResult<Schedule>> CreateSchedule([FromBody] ScheduleDto dto)
        {
            var schedule = new Schedule
            {
                WorkerId = dto.WorkerId ?? 0,
                CaseId = dto.CaseId,
                Description = dto.Description,
                EventName = dto.EventName ?? string.Empty,
                EventType = dto.EventType ?? string.Empty,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Priority = dto.Priority,
                Status = dto.Status
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            // 回傳成功新增的資料
            return Ok(schedule); // ✅ 最穩
        }

        /// <summary>
        /// 更新指定 ID 的活動資訊
        /// HTTP PUT: /api/schedule/{id}
        /// </summary>
        /// <param name="id">活動 ID</param>
        /// <param name="dto">更新後的活動資料（DTO）</param>
        /// <returns>HTTP 狀態碼</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, ScheduleDto dto)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            // 將 DTO 中的欄位值套用到實體上
            schedule.WorkerId = dto.WorkerId ?? schedule.WorkerId;
            schedule.EventType = dto.EventType ?? schedule.EventType;
            schedule.EventName = dto.EventName ?? schedule.EventName;
            schedule.Description = dto.Description ?? schedule.Description;
            schedule.StartTime = dto.StartTime;
            schedule.EndTime = dto.EndTime;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Schedules.Any(e => e.ScheduleId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        /// <summary>
        /// 刪除指定 ID 的行事曆活動
        /// HTTP DELETE: /api/schedule/{id}
        /// </summary>
        /// <param name="id">活動 ID</param>
        /// <returns>HTTP 狀態碼</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }





        // ==================== API 請求/回應模型 ====================

        /// <summary>
        /// 建立行事曆活動的請求模型
        /// </summary>
        public class CreateScheduleRequest
        {
            public int WorkerId { get; set; }                     // 所屬社工 ID（必填）
            public int? CaseId { get; set; }                      // 關聯個案 ID（可選）
            public string Description { get; set; } = string.Empty; // 活動描述（必填）
            public DateTime StartTime { get; set; }               // 開始時間（必填）
            public DateTime EndTime { get; set; }                 // 結束時間（必填）
            public string EventType { get; set; } = "中";
            public string Priority { get; set; } = "中";          // 優先順序（預設中）
            public string Status { get; set; } = "進行中";        // 狀態（預設進行中）
        }

        /// <summary>
        /// 更新行事曆活動的請求模型（所有欄位可選）
        /// </summary>
        public class UpdateScheduleRequest
        {
            public int? CaseId { get; set; }
            public string? Description { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string? Priority { get; set; }
            public string? Status { get; set; }
        }

        /// <summary>
        /// 行事曆活動的回應模型
        /// </summary>
        public class ScheduleResponse
        {
            public int ScheduleId { get; set; }                   // 活動 ID
            public int WorkerId { get; set; }                     // 所屬社工 ID
            public int? CaseId { get; set; }                      // 關聯個案 ID
            public string Description { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Priority { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;

            public string? WorkerName { get; set; }               // 社工姓名（可選，關聯查詢）
            public string? CaseName { get; set; }                 // 個案姓名（可選，關聯查詢）
        }

        public class ScheduleDto
        {
            public int? WorkerId { get; set; }                 // 工作人員 ID
            public string? EventType { get; set; }            // 事件種類（如：個案訪問、會議）
            public string? EventName { get; set; }            // 事件名稱（如：Emergency Visit J）
            public string? Description { get; set; }          // 備註說明
            public DateTime? StartTime { get; set; }           // 開始時間
            public DateTime? EndTime { get; set; }
            public string? Priority { get; set; }
            public string? Status { get; set; }
            public int? CaseId { get; set; }
        }
    }
}