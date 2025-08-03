using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.OpenAI;
using System.Text.Json;

namespace NGO_WebAPI_Backend.Controllers.ActivityManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityImageGeneratorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ActivityImageGeneratorController> _logger;

        public ActivityImageGeneratorController(IConfiguration configuration, ILogger<ActivityImageGeneratorController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("開始測試 Azure OpenAI 連接");

                // 獲取 Azure OpenAI 配置
                var endpoint = _configuration["AzureOpenAI:Endpoint"];
                var key = _configuration["AzureOpenAI:ApiKey"];
                var deploymentName = _configuration["AzureOpenAI:DalleDeploymentName"];

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(deploymentName))
                {
                    _logger.LogError("Azure OpenAI 配置缺失");
                    return StatusCode(500, new { success = false, message = "Azure OpenAI 配置缺失" });
                }

                // 創建 Azure OpenAI 客戶端
                var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

                _logger.LogInformation("Azure OpenAI 連接測試成功");

                return Ok(new
                {
                    success = true,
                    message = "Azure OpenAI 連接正常"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure OpenAI 連接測試失敗");
                return StatusCode(500, new { success = false, message = "Azure OpenAI 連接測試失敗" });
            }
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageGenerationRequest request)
        {
            try
            {
                _logger.LogInformation("開始生成圖片，描述: {Prompt}", request.Prompt);

                // 驗證輸入
                if (string.IsNullOrWhiteSpace(request.Prompt))
                {
                    return BadRequest(new { success = false, message = "請提供圖片描述" });
                }

                // 獲取 Azure OpenAI 配置
                var endpoint = _configuration["AzureOpenAI:Endpoint"];
                var key = _configuration["AzureOpenAI:ApiKey"];
                var deploymentName = _configuration["AzureOpenAI:DalleDeploymentName"];

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(deploymentName))
                {
                    _logger.LogError("Azure OpenAI 配置缺失");
                    return StatusCode(500, new { success = false, message = "服務配置錯誤" });
                }

                // 創建 Azure OpenAI 客戶端
                var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

                // 準備圖片生成選項
                var imageGenerationOptions = new ImageGenerationOptions
                {
                    Prompt = request.Prompt,
                    Size = ImageSize.Size1024x1024,
                    Quality = ImageGenerationQuality.Standard,
                    Style = ImageGenerationStyle.Natural,
                    DeploymentName = deploymentName
                };

                // 調用 DALL-E-3 生成圖片
                var response = await client.GetImageGenerationsAsync(imageGenerationOptions);

                if (response.Value.Data.Count == 0)
                {
                    _logger.LogWarning("圖片生成失敗，沒有返回數據");
                    return StatusCode(500, new { success = false, message = "圖片生成失敗" });
                }

                var imageData = response.Value.Data[0];
                
                // 檢查返回的數據類型
                _logger.LogInformation("圖片生成返回數據類型: {Type}", imageData.GetType().Name);
                
                string imageUrl;
                
                // 檢查是否有 Base64 數據
                if (!string.IsNullOrEmpty(imageData.Base64Data))
                {
                    imageUrl = $"data:image/png;base64,{imageData.Base64Data}";
                    _logger.LogInformation("使用 Base64 數據");
                }
                // 檢查是否有 URL
                else if (!string.IsNullOrEmpty(imageData.Url?.AbsoluteUri))
                {
                    imageUrl = imageData.Url.AbsoluteUri;
                    _logger.LogInformation("使用 URL: {Url}", imageUrl);
                }
                else
                {
                    _logger.LogWarning("圖片生成失敗，既沒有 Base64 數據也沒有 URL");
                    _logger.LogWarning("圖片數據內容: {ImageData}", JsonSerializer.Serialize(imageData));
                    return StatusCode(500, new { success = false, message = "圖片生成失敗，無法獲取圖片數據" });
                }

                _logger.LogInformation("圖片生成成功");

                return Ok(new
                {
                    success = true,
                    imageData = imageUrl,
                    message = "圖片生成成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "圖片生成過程中發生錯誤");
                return StatusCode(500, new { success = false, message = "圖片生成失敗，請稍後再試" });
            }
        }
    }

    public class ImageGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
} 