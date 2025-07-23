using Azure.AI.OpenAI;
using Azure;

namespace NGO_WebAPI_Backend.Services
{
    public class AzureOpenAIService
    {
        private readonly OpenAIClient? _openAIClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AzureOpenAIService(IConfiguration configuration, ILogger<AzureOpenAIService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var apiKey = _configuration["AzureOpenAI:ApiKey"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Azure OpenAI 設定缺失，AI 優化功能將無法使用");
                return;
            }

            _openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        /// <summary>
        /// 優化活動描述文案
        /// </summary>
        /// <param name="originalDescription">原始活動描述</param>
        /// <returns>優化後的活動描述</returns>
        public async Task<string> OptimizeActivityDescription(string originalDescription)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originalDescription))
                {
                    throw new ArgumentException("活動描述不能為空", nameof(originalDescription));
                }

                if (_openAIClient == null)
                {
                    throw new InvalidOperationException("Azure OpenAI 服務未正確配置");
                }

                _logger.LogInformation("開始 AI 優化活動描述，原始內容長度: {Length}", originalDescription.Length);

                var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4";

                var systemPrompt = @"您是一位專業的NGO活動文案專家。請將使用者提供的簡單活動描述改寫成生動、具體、吸引人的版本。

要求：
1. 保持原意和核心資訊不變
2. 增加情感色彩和具體細節
3. 突出活動的社會意義和正面影響
4. 使用溫暖、正面、鼓舞人心的語調
5. 讓讀者能感受到參與的價值和意義
6. 控制在150-300字之間
7. 使用繁體中文回應
8. 避免過度誇張，保持真實可信

請直接提供優化後的活動描述，不需要額外說明。";

                var userPrompt = $"請優化以下活動描述：\n\n{originalDescription}";

                // 使用新的 OpenAI SDK API
                var messages = new List<ChatRequestMessage>
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(userPrompt)
                };

                var chatCompletionOptions = new ChatCompletionsOptions(deploymentName, messages);
                var chatCompletion = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

                if (chatCompletion?.Value?.Choices?.Count > 0)
                {
                    var optimizedText = chatCompletion.Value.Choices[0].Message.Content?.Trim() ?? "";
                    
                    _logger.LogInformation("AI 優化完成，優化後內容長度: {Length}", optimizedText.Length);
                    
                    return optimizedText;
                }

                throw new InvalidOperationException("AI 未返回有效回應");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 優化活動描述時發生錯誤");
                throw new InvalidOperationException($"AI 優化失敗: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 檢查 AI 服務是否可用
        /// </summary>
        /// <returns>是否可用</returns>
        public bool IsAvailable()
        {
            return _openAIClient != null && 
                   !string.IsNullOrEmpty(_configuration["AzureOpenAI:Endpoint"]) &&
                   !string.IsNullOrEmpty(_configuration["AzureOpenAI:ApiKey"]);
        }
    }
}