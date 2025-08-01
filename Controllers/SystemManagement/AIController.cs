using Microsoft.AspNetCore.Mvc;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AzureOpenAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(AzureOpenAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// 優化活動描述
        /// </summary>
        [HttpPost("optimize-description")]
        public async Task<ActionResult<OptimizeDescriptionResponse>> OptimizeDescription([FromBody] OptimizeDescriptionRequest request)
        {
            try
            {
                _logger.LogInformation("收到活動描述優化請求");

                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    return BadRequest(new { message = "活動描述不能為空" });
                }

                if (request.Description.Length > 1000)
                {
                    return BadRequest(new { message = "活動描述長度不能超過 1000 字元" });
                }

                if (!_aiService.IsAvailable())
                {
                    return StatusCode(503, new { message = "AI 服務暫時無法使用，請稍後再試" });
                }

                var optimizedDescription = await _aiService.OptimizeActivityDescription(request.Description);

                _logger.LogInformation("活動描述優化成功");

                return Ok(new OptimizeDescriptionResponse
                {
                    OriginalDescription = request.Description,
                    OptimizedDescription = optimizedDescription,
                    OriginalLength = request.Description.Length,
                    OptimizedLength = optimizedDescription.Length,
                    OptimizedAt = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "請求參數錯誤");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "AI 服務錯誤");
                return StatusCode(500, new { message = "AI 優化失敗，請稍後再試" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "優化活動描述時發生未預期錯誤");
                return StatusCode(500, new { message = "系統錯誤，請稍後再試" });
            }
        }

        /// <summary>
        /// 檢查 AI 服務狀態
        /// </summary>
        [HttpGet("status")]
        public ActionResult<AIServiceStatusResponse> GetAIServiceStatus()
        {
            var isAvailable = _aiService.IsAvailable();
            
            return Ok(new AIServiceStatusResponse
            {
                IsAvailable = isAvailable,
                Message = isAvailable ? "AI 服務正常運作" : "AI 服務暫時無法使用",
                CheckedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 優化描述請求模型
    /// </summary>
    public class OptimizeDescriptionRequest
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 優化描述回應模型
    /// </summary>
    public class OptimizeDescriptionResponse
    {
        public string OriginalDescription { get; set; } = string.Empty;
        public string OptimizedDescription { get; set; } = string.Empty;
        public int OriginalLength { get; set; }
        public int OptimizedLength { get; set; }
        public DateTime OptimizedAt { get; set; }
    }

    /// <summary>
    /// AI 服務狀態回應模型
    /// </summary>
    public class AIServiceStatusResponse
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
    }
}