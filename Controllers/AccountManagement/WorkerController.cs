using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend.Controllers.AccountManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly IPasswordService _passwordService;

        public WorkerController(NgoplatformDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
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
        /// 創建新工作人員
        /// </summary>
        /// <param name="request">工作人員創建請求</param>
        /// <returns>創建結果</returns>
        [HttpPost]
        public async Task<ActionResult<object>> CreateWorker([FromBody] CreateWorkerRequest request)
        {
            try
            {
                // 檢查Email是否已存在
                var existingWorker = await _context.Workers
                    .Where(w => w.Email == request.Email)
                    .FirstOrDefaultAsync();

                if (existingWorker != null)
                {
                    return BadRequest(new { success = false, message = "此Email已被使用" });
                }

                // 雜湊密碼
                var hashedPassword = _passwordService.HashPassword(request.Password);

                // 創建新工作人員
                var worker = new Worker
                {
                    Email = request.Email,
                    Password = hashedPassword,
                    Name = request.Name,
                    Role = request.Role
                };

                _context.Workers.Add(worker);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "工作人員創建成功",
                    worker = new
                    {
                        workerId = worker.WorkerId,
                        email = worker.Email,
                        name = worker.Name,
                        role = worker.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "創建工作人員失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新工作人員密碼
        /// </summary>
        /// <param name="id">工作人員ID</param>
        /// <param name="request">密碼更新請求</param>
        /// <returns>更新結果</returns>
        [HttpPut("{id}/password")]
        public async Task<ActionResult<object>> UpdatePassword(int id, [FromBody] UpdatePasswordRequest request)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(id);
                if (worker == null)
                {
                    return NotFound(new { success = false, message = "找不到工作人員" });
                }

                // 雜湊新密碼
                worker.Password = _passwordService.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "密碼更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "密碼更新失敗", error = ex.Message });
            }
        }
    }


    /// <summary>
    /// 創建工作人員請求模型
    /// </summary>
    public class CreateWorkerRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "staff";
    }

    /// <summary>
    /// 更新密碼請求模型
    /// </summary>
    public class UpdatePasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}