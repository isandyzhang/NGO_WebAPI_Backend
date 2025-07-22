using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public WorkerController(NgoplatformDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根據 Email 查詢工作人員資訊
        /// </summary>
        /// <param name="email">工作人員 Email</param>
        /// <returns>工作人員資訊</returns>
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<object>> GetWorkerByEmail(string email)
        {
            try
            {
                var worker = await _context.Workers
                    .Where(w => w.Email == email)
                    .Select(w => new
                    {
                        workerId = w.WorkerId,
                        email = w.Email,
                        name = w.Name,
                        role = w.Role ?? "staff" // 預設為 staff 角色
                    })
                    .FirstOrDefaultAsync();

                if (worker == null)
                {
                    return NotFound(new { message = "找不到對應的工作人員" });
                }

                return Ok(worker);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "查詢工作人員資訊失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 根據 WorkerId 查詢工作人員資訊
        /// </summary>
        /// <param name="id">工作人員 ID</param>
        /// <returns>工作人員資訊</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetWorkerById(int id)
        {
            try
            {
                var worker = await _context.Workers
                    .Where(w => w.WorkerId == id)
                    .Select(w => new
                    {
                        workerId = w.WorkerId,
                        email = w.Email,
                        name = w.Name,
                        role = w.Role ?? "staff" // 預設為 staff 角色
                    })
                    .FirstOrDefaultAsync();

                if (worker == null)
                {
                    return NotFound(new { message = "找不到對應的工作人員" });
                }

                return Ok(worker);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "查詢工作人員資訊失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 取得所有工作人員列表
        /// </summary>
        /// <returns>工作人員列表</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetWorkers()
        {
            try
            {
                var workers = await _context.Workers
                    .Select(w => new
                    {
                        workerId = w.WorkerId,
                        email = w.Email,
                        name = w.Name,
                        role = w.Role ?? "staff" // 預設為 staff 角色
                    })
                    .ToListAsync();

                return Ok(workers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得工作人員列表失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 工作人員登入驗證
        /// </summary>
        /// <param name="loginRequest">登入請求</param>
        /// <returns>登入結果</returns>
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] TestLoginRequest loginRequest)
        {
            try
            {
                // 查詢工作人員
                var worker = await _context.Workers
                    .Where(w => w.Email == loginRequest.Email)
                    .Select(w => new
                    {
                        workerId = w.WorkerId,
                        email = w.Email,
                        name = w.Name,
                        role = w.Role ?? "staff",
                        password = w.Password
                    })
                    .FirstOrDefaultAsync();

                if (worker == null)
                {
                    return BadRequest(new { success = false, message = "找不到對應的工作人員帳號" });
                }

                // 簡單的密碼驗證 (實際應用中應該使用密碼雜湊)
                if (worker.password != loginRequest.Password)
                {
                    return BadRequest(new { success = false, message = "密碼錯誤" });
                }

                // 登入成功，返回工作人員資訊 (不包含密碼)
                var workerInfo = new
                {
                    workerId = worker.workerId,
                    email = worker.email,
                    name = worker.name,
                    role = worker.role
                };

                return Ok(new
                {
                    success = true,
                    message = "登入成功",
                    worker = workerInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "登入過程發生錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 測試端點 - 檢查工作人員密碼（僅供開發測試）
        /// </summary>
        [HttpPost("test-password")]
        public async Task<ActionResult<object>> TestPassword([FromBody] TestLoginRequest loginRequest)
        {
            try
            {
                var worker = await _context.Workers
                    .Where(w => w.Email == loginRequest.Email)
                    .Select(w => new
                    {
                        workerId = w.WorkerId,
                        email = w.Email,
                        name = w.Name,
                        role = w.Role ?? "staff",
                        password = w.Password,
                        hasPassword = !string.IsNullOrEmpty(w.Password)
                    })
                    .FirstOrDefaultAsync();

                if (worker == null)
                {
                    return NotFound(new { success = false, message = "找不到對應的工作人員帳號" });
                }

                return Ok(new
                {
                    success = true,
                    worker = new
                    {
                        worker.workerId,
                        worker.email,
                        worker.name,
                        worker.role,
                        worker.hasPassword,
                        passwordMatch = worker.password == loginRequest.Password,
                        storedPassword = worker.password // 注意：生產環境中不應返回密碼
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "測試過程發生錯誤", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// 測試登入請求模型
    /// </summary>
    public class TestLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}