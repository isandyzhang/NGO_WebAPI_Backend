using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="logger">記錄器</param>
        public AuthController(NgoplatformDbContext context, ILogger<AuthController> logger)
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
                        Email = worker.Email ?? string.Empty,
                        Name = worker.Name ?? string.Empty,
                        Role = worker.Role ?? string.Empty
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
        /// 驗證Email是否存在
        /// </summary>
        /// <param name="request">Email驗證請求</param>
        /// <returns>驗證結果</returns>
        [HttpPost("verify-email")]
        public async Task<ActionResult<EmailVerificationResponse>> VerifyEmail([FromBody] EmailVerificationRequest request)
        {
            try
            {
                _logger.LogInformation("開始驗證Email是否存在: {Email}", request.Email);

                // 驗證輸入資料
                if (string.IsNullOrEmpty(request.Email))
                {
                    _logger.LogWarning("Email驗證失敗：Email為空");
                    return Ok(new EmailVerificationResponse
                    {
                        Success = false,
                        Message = "請輸入Email地址"
                    });
                }

                // 簡單的Email格式驗證
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
                if (!emailRegex.IsMatch(request.Email))
                {
                    _logger.LogWarning("Email驗證失敗：格式不正確 {Email}", request.Email);
                    return Ok(new EmailVerificationResponse
                    {
                        Success = false,
                        Message = "請輸入有效的Email地址"
                    });
                }

                // 查詢工作人員是否存在
                var worker = await _context.Workers
                    .Where(w => w.Email == request.Email)
                    .FirstOrDefaultAsync();

                if (worker == null)
                {
                    _logger.LogWarning("Email驗證失敗：找不到該Email的工作人員 {Email}", request.Email);
                    return Ok(new EmailVerificationResponse
                    {
                        Success = false,
                        Message = "此Email尚未註冊，請聯絡管理員"
                    });
                }

                // Email驗證成功
                _logger.LogInformation("Email驗證成功 {Email}", request.Email);
                return Ok(new EmailVerificationResponse
                {
                    Success = true,
                    Message = "Email驗證成功，請輸入密碼"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email驗證過程中發生錯誤");
                return Ok(new EmailVerificationResponse
                {
                    Success = false,
                    Message = "驗證過程中發生錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 取得所有工作人員列表
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
                        Email = w.Email ?? string.Empty,
                        Name = w.Name ?? string.Empty,
                        Role = w.Role ?? string.Empty
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


    }

    /// <summary>
    /// 登入請求模型
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登入回應模型
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public WorkerInfo? Worker { get; set; }
    }

    /// <summary>
    /// 工作人員資訊模型
    /// </summary>
    public class WorkerInfo
    {
        public int WorkerId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Email驗證請求模型
    /// </summary>
    public class EmailVerificationRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Email驗證回應模型
    /// </summary>
    public class EmailVerificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 