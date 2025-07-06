using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Data;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    /// <summary>
    /// 身份驗證控制器 - 處理登入相關功能
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="logger">記錄器</param>
        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 工作人員登入
        /// </summary>
        /// <param name="request">登入請求</param>
        /// <returns>登入結果</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("開始處理登入請求，Email: {Email}", request.Email);

                // 驗證輸入資料
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("登入失敗：Email或密碼為空");
                    return Ok(new LoginResponse
                    {
                        Success = false,
                        Message = "請輸入Email和密碼"
                    });
                }

                // 查詢工作人員
                var worker = await _context.Workers
                    .Where(w => w.Email == request.Email)
                    .FirstOrDefaultAsync();

                if (worker == null)
                {
                    _logger.LogWarning("登入失敗：找不到該Email的工作人員 {Email}", request.Email);
                    return Ok(new LoginResponse
                    {
                        Success = false,
                        Message = "Email或密碼錯誤"
                    });
                }

                // 驗證密碼（這裡使用簡單的字串比對，實際專案中應該使用雜湊密碼）
                if (worker.Password != request.Password)
                {
                    _logger.LogWarning("登入失敗：密碼錯誤 {Email}", request.Email);
                    return Ok(new LoginResponse
                    {
                        Success = false,
                        Message = "Email或密碼錯誤"
                    });
                }

                // 登入成功
                _logger.LogInformation("登入成功 {Email}", request.Email);
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "登入成功",
                    Worker = new WorkerInfo
                    {
                        WorkerId = worker.WorkerId,
                        Email = worker.Email,
                        Name = worker.Name
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入過程中發生錯誤");
                return Ok(new LoginResponse
                {
                    Success = false,
                    Message = "登入過程中發生錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 取得所有工作人員列表（測試用）
        /// </summary>
        /// <returns>工作人員列表</returns>
        [HttpGet("workers")]
        public async Task<ActionResult<IEnumerable<WorkerInfo>>> GetWorkers()
        {
            try
            {
                var workers = await _context.Workers
                    .Select(w => new WorkerInfo
                    {
                        WorkerId = w.WorkerId,
                        Email = w.Email,
                        Name = w.Name
                    })
                    .ToListAsync();

                return Ok(workers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得工作人員列表時發生錯誤");
                return StatusCode(500, "取得工作人員列表時發生錯誤");
            }
        }

        /// <summary>
        /// 取得工作人員詳細資料（包含密碼，僅用於測試）
        /// </summary>
        /// <returns>工作人員詳細資料</returns>
        [HttpGet("workers-detail")]
        public async Task<ActionResult<IEnumerable<object>>> GetWorkersDetail()
        {
            try
            {
                var workers = await _context.Workers
                    .Select(w => new
                    {
                        w.WorkerId,
                        w.Email,
                        w.Password,
                        w.Name
                    })
                    .ToListAsync();

                return Ok(workers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得工作人員詳細資料時發生錯誤");
                return StatusCode(500, "取得工作人員詳細資料時發生錯誤");
            }
        }
    }
} 