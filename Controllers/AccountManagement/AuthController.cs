using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NGO_WebAPI_Backend.Controllers.AccountManagement
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
        private readonly IConfiguration _configuration;
        private readonly IPasswordService _passwordService;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="logger">記錄器</param>
        /// <param name="configuration">配置</param>
        /// <param name="passwordService">密碼服務</param>
        public AuthController(NgoplatformDbContext context, ILogger<AuthController> logger, IConfiguration configuration, IPasswordService passwordService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _passwordService = passwordService;
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

                // 使用Argon2驗證密碼
                if (!_passwordService.VerifyPassword(request.Password, worker.Password ?? ""))
                {
                    _logger.LogWarning("登入失敗：密碼錯誤 {Email}", request.Email);
                    return Ok(new LoginResponse
                    {
                        Success = false,
                        Message = "Email或密碼錯誤"
                    });
                }

                // 生成 JWT token
                var token = GenerateJwtToken(worker);

                // 登入成功
                _logger.LogInformation("登入成功 {Email}", request.Email);
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "登入成功",
                    Token = token,
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
        /// Azure用戶登入後自動註冊到本地資料庫
        /// </summary>
        /// <param name="request">Azure用戶註冊請求</param>
        /// <returns>註冊結果</returns>
        [HttpPost("azure-user-sync")]
        public async Task<ActionResult<AzureUserSyncResponse>> SyncAzureUser([FromBody] AzureUserSyncRequest request)
        {
            try
            {
                _logger.LogInformation("Azure用戶同步開始，Email: {Email}", request.Email);

                // 驗證輸入資料
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.DisplayName))
                {
                    _logger.LogWarning("Azure用戶同步失敗：Email或DisplayName為空");
                    return Ok(new AzureUserSyncResponse
                    {
                        Success = false,
                        Message = "Email和DisplayName為必填欄位"
                    });
                }

                // 檢查是否已存在該Email的工作人員
                var existingWorker = await _context.Workers
                    .Where(w => w.Email == request.Email)
                    .FirstOrDefaultAsync();

                if (existingWorker != null)
                {
                    _logger.LogInformation("Azure用戶已存在於資料庫中，WorkerId: {WorkerId}", existingWorker.WorkerId);
                    
                    // 如果已存在，返回現有的工作人員資訊並生成JWT
                    var existingToken = GenerateJwtToken(existingWorker);
                    
                    return Ok(new AzureUserSyncResponse
                    {
                        Success = true,
                        Message = "用戶已存在，登入成功",
                        Token = existingToken,
                        Worker = new WorkerInfo
                        {
                            WorkerId = existingWorker.WorkerId,
                            Email = existingWorker.Email ?? string.Empty,
                            Name = existingWorker.Name ?? string.Empty,
                            Role = existingWorker.Role ?? "staff"
                        }
                    });
                }

                // 創建新的Worker記錄
                var defaultPassword = "AzureUser@" + DateTime.Now.ToString("yyyyMMdd"); // 預設密碼
                var hashedPassword = _passwordService.HashPassword(defaultPassword);

                var newWorker = new Worker
                {
                    Email = request.Email,
                    Name = request.DisplayName,
                    Role = "staff", // Azure用戶預設為staff權限
                    Password = hashedPassword
                };

                _context.Workers.Add(newWorker);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Azure用戶創建成功，WorkerId: {WorkerId}, Email: {Email}", 
                    newWorker.WorkerId, newWorker.Email);

                // 生成JWT Token
                var token = GenerateJwtToken(newWorker);

                return Ok(new AzureUserSyncResponse
                {
                    Success = true,
                    Message = "Azure用戶已自動註冊並登入成功",
                    Token = token,
                    Worker = new WorkerInfo
                    {
                        WorkerId = newWorker.WorkerId,
                        Email = newWorker.Email ?? string.Empty,
                        Name = newWorker.Name ?? string.Empty,
                        Role = newWorker.Role ?? "staff"
                    },
                    DefaultPassword = defaultPassword // 返回預設密碼，讓用戶知道可以變更
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure用戶同步過程中發生錯誤");
                return Ok(new AzureUserSyncResponse
                {
                    Success = false,
                    Message = "用戶同步過程中發生錯誤，請稍後再試"
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

        /// <summary>
        /// 生成 JWT Token
        /// </summary>
        /// <param name="worker">工作人員資訊</param>
        /// <returns>JWT Token</returns>
        private string GenerateJwtToken(Worker worker)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "NGO_Platform_Super_Secret_Key_For_Development_Only_2024";
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, worker.WorkerId.ToString()),
                    new Claim(ClaimTypes.Email, worker.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, worker.Name ?? string.Empty),
                    new Claim(ClaimTypes.Role, worker.Role ?? string.Empty),
                    new Claim("WorkerId", worker.WorkerId.ToString()) // 自定義 claim
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token 有效期 7 天
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
        public string? Token { get; set; }
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

    /// <summary>
    /// Azure用戶同步請求模型
    /// </summary>
    public class AzureUserSyncRequest
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? TenantId { get; set; }
    }

    /// <summary>
    /// Azure用戶同步回應模型
    /// </summary>
    public class AzureUserSyncResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public WorkerInfo? Worker { get; set; }
        public string? DefaultPassword { get; set; }
    }
} 