using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure.Storage.Blobs;
using System.Text;
using NGO_WebAPI_Backend.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace NGO_WebAPI_Backend.Controllers.CaseManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaseSpeechToTextController : ControllerBase
    {
        private readonly ILogger<CaseSpeechToTextController> _logger;
        private readonly IConfiguration _configuration;
        private readonly NgoplatformDbContext _context;

        public CaseSpeechToTextController(ILogger<CaseSpeechToTextController> logger, IConfiguration configuration, NgoplatformDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// 上傳音檔到 Azure Blob Storage 並儲存到資料庫
        /// </summary>
        [HttpPost("upload-audio")]
        public async Task<ActionResult<AudioUploadResponse>> UploadAudio(IFormFile audioFile, [FromForm] int? caseId = null)
        {
            try
            {
                _logger.LogInformation($"開始處理音檔上傳請求 - caseId: {caseId}");
                
                // 記錄所有 Form 資料
                _logger.LogInformation("Form 資料內容:");
                foreach (var formField in Request.Form)
                {
                    _logger.LogInformation($"  {formField.Key}: {formField.Value}");
                }

                if (audioFile == null || audioFile.Length == 0)
                {
                    return BadRequest(new { message = "未提供音檔" });
                }

                // 檢查檔案大小 (限制為 25MB)
                if (audioFile.Length > 25 * 1024 * 1024)
                {
                    return BadRequest(new { message = "音檔大小不能超過 25MB" });
                }

                // 檢查檔案格式
                var allowedExtensions = new[] { ".wav", ".mp3", ".m4a", ".webm", ".ogg" };
                var fileExtension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "不支援的音檔格式，請使用 WAV、MP3、M4A、WEBM 或 OGG 格式" });
                }

                // 上傳音檔到 Azure Blob Storage
                string audioUrl;
                try
                {
                    audioUrl = await UploadAudioToBlobStorage(audioFile);
                    _logger.LogInformation($"音檔上傳成功，URL: {audioUrl}");
                }
                catch (Exception uploadEx)
                {
                    _logger.LogError(uploadEx, "音檔上傳到 Azure Blob Storage 失敗");
                    return StatusCode(500, new { 
                        message = "音檔上傳失敗", 
                        error = uploadEx.Message,
                        details = "請檢查 Azure Storage 設定和網路連線"
                    });
                }

                // 如果提供了 caseId，更新個案的音檔 URL
                if (caseId.HasValue)
                {
                    _logger.LogInformation($"準備更新個案 ID: {caseId} 的音檔 URL");
                    var caseItem = await _context.Cases.FindAsync(caseId.Value);
                    if (caseItem != null)
                    {
                        _logger.LogInformation($"找到個案 ID: {caseId}，更新音檔 URL: {audioUrl}");
                        caseItem.SpeechToTextAudioUrl = audioUrl;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"已成功更新個案 ID: {caseId} 的音檔 URL");
                    }
                    else
                    {
                        _logger.LogWarning($"找不到個案 ID: {caseId}，無法更新音檔 URL");
                    }
                }
                else
                {
                    _logger.LogInformation("未提供 caseId，跳過個案更新");
                }

                _logger.LogInformation($"音檔上傳成功，URL: {audioUrl}");

                return Ok(new AudioUploadResponse
                {
                    AudioUrl = audioUrl,
                    FileName = audioFile.FileName,
                    FileSize = audioFile.Length,
                    UploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "音檔上傳處理失敗");
                return StatusCode(500, new { message = "音檔上傳失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 語音轉文字
        /// </summary>
        [HttpPost("transcribe")]
        public async Task<ActionResult<SpeechToTextResponse>> TranscribeAudio(IFormFile audioFile)
        {
            try
            {
                _logger.LogInformation("開始處理語音轉文字請求");

                if (audioFile == null || audioFile.Length == 0)
                {
                    return BadRequest(new { message = "未提供音檔" });
                }

                // 檢查檔案大小 (限制為 25MB)
                if (audioFile.Length > 25 * 1024 * 1024)
                {
                    return BadRequest(new { message = "音檔大小不能超過 25MB" });
                }

                // 檢查檔案格式
                var allowedExtensions = new[] { ".wav", ".mp3", ".m4a", ".webm", ".ogg" };
                var fileExtension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "不支援的音檔格式，請使用 WAV、MP3、M4A、WEBM 或 OGG 格式" });
                }

                // 取得 Azure Speech Service 設定
                var speechKey = _configuration["AzureSpeech:Key"];
                var speechRegion = _configuration["AzureSpeech:Region"];

                if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
                {
                    _logger.LogError("Azure Speech Service 設定缺失");
                    return StatusCode(500, new { message = "語音服務設定錯誤" });
                }

                // 將音檔轉換為位元組陣列
                using var memoryStream = new MemoryStream();
                await audioFile.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();

                // 使用 Azure Speech Service 進行語音識別
                var result = await TranscribeAudioAsync(audioBytes, speechKey, speechRegion);

                _logger.LogInformation($"語音轉文字成功，識別結果：{result.Text}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "語音轉文字處理失敗");
                return StatusCode(500, new { message = "語音轉文字失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 測試 Azure Blob Storage 連線
        /// </summary>
        [HttpGet("test-blob-storage")]
        public async Task<IActionResult> TestBlobStorageConnection()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerName = _configuration["AzureStorage:ContainerName"];
                var audioFolder = _configuration["AzureStorage:AudioFolder"] ?? "case_audio/";

                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest(new { message = "Azure Storage 連接字串未配置" });
                }

                if (string.IsNullOrEmpty(containerName))
                {
                    return BadRequest(new { message = "Azure Storage 容器名稱未配置" });
                }

                _logger.LogInformation($"測試 Azure Blob Storage 連線... Container: {containerName}, AudioFolder: {audioFolder}");

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // 檢查容器是否存在
                var containerExists = await containerClient.ExistsAsync();
                if (!containerExists.Value)
                {
                    _logger.LogInformation($"容器 {containerName} 不存在，正在創建...");
                    await containerClient.CreateIfNotExistsAsync();
                    _logger.LogInformation($"容器 {containerName} 創建成功");
                }
                else
                {
                    _logger.LogInformation($"容器 {containerName} 已存在");
                }

                // 列出容器中的檔案
                var blobs = new List<string>();
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    blobs.Add(blobItem.Name);
                }

                return Ok(new { 
                    message = "Azure Blob Storage 連線測試成功",
                    containerName = containerName,
                    audioFolder = audioFolder,
                    containerExists = containerExists.Value,
                    blobCount = blobs.Count,
                    blobs = blobs.Take(10).ToList() // 只顯示前10個檔案
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Blob Storage 連線測試失敗");
                return StatusCode(500, new { 
                    message = "Azure Blob Storage 連線測試失敗", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// 測試 Azure Speech Service 連線
        /// </summary>
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestAzureSpeechConnection()
        {
            try
            {
                var speechKey = _configuration["AzureSpeech:Key"];
                var speechRegion = _configuration["AzureSpeech:Region"];

                if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
                {
                    return BadRequest(new { message = "Azure Speech Service 設定缺失" });
                }
                
                _logger.LogInformation($"測試 Azure Speech Service 連線... Key: {speechKey.Substring(0, 10)}..., Region: {speechRegion}");

                var config = SpeechConfig.FromSubscription(speechKey, speechRegion);
                config.SpeechRecognitionLanguage = "zh-TW";

                // 建立空的音訊設定來測試連線
                using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                // 簡單的連線測試
                await Task.Delay(100); // 小延遲確保設定完成

                return Ok(new { 
                    message = "Azure Speech Service 連線設定正常",
                    key = speechKey.Substring(0, 10) + "...",
                    region = speechRegion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Speech Service 連線測試失敗");
                return StatusCode(500, new { message = "連線測試失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 從 URL 進行語音轉文字
        /// </summary>
        [HttpPost("transcribe-from-url")]
        public async Task<ActionResult<SpeechToTextResponse>> TranscribeFromUrl([FromBody] TranscribeFromUrlRequest request)
        {
            try
            {
                _logger.LogInformation($"開始從 URL 進行語音轉文字: {request.AudioUrl}");

                if (string.IsNullOrEmpty(request.AudioUrl))
                {
                    return BadRequest(new { message = "未提供音檔 URL" });
                }

                // 取得 Azure Speech Service 設定
                var speechKey = _configuration["AzureSpeech:Key"];
                var speechRegion = _configuration["AzureSpeech:Region"];

                _logger.LogInformation($"Azure Speech Service 設定 - Key: {speechKey?.Substring(0, 10)}..., Region: {speechRegion}");

                if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
                {
                    _logger.LogError("Azure Speech Service 設定缺失");
                    return StatusCode(500, new { message = "語音服務設定錯誤" });
                }

                // 下載音檔
                using var httpClient = new HttpClient();
                _logger.LogInformation($"開始下載音檔: {request.AudioUrl}");
                var audioBytes = await httpClient.GetByteArrayAsync(request.AudioUrl);
                _logger.LogInformation($"音檔下載完成，大小: {audioBytes.Length} bytes");
                
                // 檢查音檔前幾個位元組，判斷格式
                if (audioBytes.Length > 12)
                {
                    var headerBytes = audioBytes.Take(12).ToArray();
                    var headerHex = BitConverter.ToString(headerBytes);
                    _logger.LogInformation($"音檔標頭 (Hex): {headerHex}");
                    
                    // 檢查是否為 WAV 格式 (RIFF...WAVE)
                    if (audioBytes.Length >= 12 && 
                        headerBytes[0] == 0x52 && headerBytes[1] == 0x49 && // "RI"
                        headerBytes[2] == 0x46 && headerBytes[3] == 0x46 && // "FF"
                        headerBytes[8] == 0x57 && headerBytes[9] == 0x41 && // "WA"
                        headerBytes[10] == 0x56 && headerBytes[11] == 0x45) // "VE"
                    {
                        _logger.LogInformation("檢測到 WAV 格式音檔");
                    }
                    else
                    {
                        _logger.LogWarning("音檔格式可能不是標準 WAV，這可能導致識別失敗");
                    }
                }
                
                // 驗證音檔不是空的或損壞的
                if (audioBytes.Length < 1000)
                {
                    _logger.LogWarning($"音檔太小，可能有問題: {audioBytes.Length} bytes");
                }

                // 使用 Azure Speech Service 進行語音識別
                _logger.LogInformation("開始進行語音識別...");
                var result = await TranscribeAudioAsync(audioBytes, speechKey, speechRegion);

                _logger.LogInformation($"從 URL 語音轉文字成功，識別結果：{result.Text}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從 URL 語音轉文字處理失敗");
                return StatusCode(500, new { message = "語音轉文字失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 使用 Azure Speech Service 進行語音識別
        /// </summary>
        private async Task<SpeechToTextResponse> TranscribeAudioAsync(byte[] audioBytes, string speechKey, string speechRegion)
        {
            try
            {
                _logger.LogInformation($"開始配置 Azure Speech Service... Key: {speechKey.Substring(0, 10)}..., Region: {speechRegion}");
                
                // 測試 Azure Speech Service 連線
                _logger.LogInformation($"使用 Speech Service 配置 - Key: {speechKey.Substring(0, 10)}..., Region: {speechRegion}");
                
                var config = SpeechConfig.FromSubscription(speechKey, speechRegion);
                
                // 添加更詳細的錯誤處理
                try
                {
                    // 測試配置是否有效
                    config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "3000");
                }
                catch (Exception configEx)
                {
                    _logger.LogError(configEx, "Speech Service 配置失敗");
                    throw new Exception($"Speech Service 配置錯誤: {configEx.Message}");
                }
                
                // 設定語音識別參數 - 支援多語言自動偵測
                config.SpeechRecognitionLanguage = "zh-TW"; // 主要語言：繁體中文
                config.OutputFormat = OutputFormat.Detailed;
                
                // 增加多語言支援
                var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(
                    new string[] { "zh-TW", "zh-CN", "en-US" });
                
                // 調整音訊超時設定，給更多時間處理
                config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "3000");
                config.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "10000");
                
                // 啟用詳細日誌
                config.SetProperty(PropertyId.Speech_LogFilename, "speech.log");

                _logger.LogInformation("開始創建音訊流...");
                
                // 讓 Azure 自動檢測音檔格式
                var audioStream = AudioInputStream.CreatePushStream();
                _logger.LogInformation("使用預設音訊流格式 - 讓 Azure 自動檢測");

                using var audioConfig = AudioConfig.FromStreamInput(audioStream);
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                var tcs = new TaskCompletionSource<SpeechToTextResponse>();
                var startTime = DateTime.UtcNow;
                var hasResult = false;

                // 監聽語音識別過程中的事件
                recognizer.Recognizing += (s, e) =>
                {
                    _logger.LogInformation($"正在識別語音: '{e.Result.Text}'");
                };

                // 設定識別結果處理
                recognizer.Recognized += (s, e) =>
                {
                    _logger.LogInformation($"語音識別結果: '{e.Result.Text}', 原因: {e.Result.Reason}");
                    
                    if (e.Result.Reason == ResultReason.RecognizedSpeech && !hasResult)
                    {
                        hasResult = true;
                        var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                        var confidence = string.IsNullOrEmpty(e.Result.Text) ? 0.0 : 0.8;
                        var resultText = string.IsNullOrEmpty(e.Result.Text) ? "無法識別語音內容" : e.Result.Text;

                        tcs.TrySetResult(new SpeechToTextResponse
                        {
                            Text = resultText,
                            Confidence = confidence,
                            Duration = duration
                        });
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        _logger.LogWarning("語音識別無匹配結果");
                        if (!hasResult)
                        {
                            hasResult = true;
                            tcs.TrySetResult(new SpeechToTextResponse
                            {
                                Text = "無法識別語音內容 - 沒有找到匹配的語音",
                                Confidence = 0.0,
                                Duration = (DateTime.UtcNow - startTime).TotalSeconds
                            });
                        }
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    _logger.LogWarning($"語音識別被取消：{e.Reason}, 詳細: {e.ErrorDetails}");
                    
                    // 如果是認證錯誤，提供更清楚的錯誤訊息
                    if (e.ErrorDetails.Contains("Authentication error") || e.ErrorDetails.Contains("401"))
                    {
                        _logger.LogError("Azure Speech Service 認證失敗，請檢查 API 金鑰和 Region 設定");
                        if (!hasResult)
                        {
                            tcs.TrySetException(new Exception("Azure Speech Service 認證失敗，請檢查 API 金鑰和 Region 設定"));
                        }
                    }
                    else if (!hasResult)
                    {
                        tcs.TrySetException(new Exception($"語音識別被取消：{e.Reason} - {e.ErrorDetails}"));
                    }
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    _logger.LogInformation("語音識別會話結束");
                    if (!hasResult)
                    {
                        tcs.TrySetResult(new SpeechToTextResponse
                        {
                            Text = "無法識別語音內容",
                            Confidence = 0.0,
                            Duration = (DateTime.UtcNow - startTime).TotalSeconds
                        });
                    }
                };

                _logger.LogInformation("嘗試寫入臨時檔案進行識別...");
                
                // 將音檔寫入臨時檔案，使用檔案識別可能更穩定
                var tempFileName = Path.GetTempFileName() + ".wav";
                await System.IO.File.WriteAllBytesAsync(tempFileName, audioBytes);
                _logger.LogInformation($"臨時音檔建立: {tempFileName}");
                
                SpeechRecognitionResult result;
                try
                {
                    // 嘗試從檔案識別
                    using var audioConfigFromFile = AudioConfig.FromWavFileInput(tempFileName);
                    using var recognizerFromFile = new SpeechRecognizer(config, audioConfigFromFile);
                    
                    _logger.LogInformation("開始從檔案進行語音識別...");
                    result = await recognizerFromFile.RecognizeOnceAsync();
                    _logger.LogInformation($"檔案識別結果: '{result.Text}', 原因: {result.Reason}");
                }
                catch (Exception fileEx)
                {
                    _logger.LogWarning($"檔案識別失敗，回到流識別: {fileEx.Message}");
                    
                    // 如果檔案識別失敗，回到原來的流識別
                    _logger.LogInformation($"推送音訊資料，大小: {audioBytes.Length} bytes");
                    if (audioStream is PushAudioInputStream pushStream)
                    {
                        pushStream.Write(audioBytes);
                        pushStream.Close();
                    }

                    _logger.LogInformation("開始單次語音識別...");
                    result = await recognizer.RecognizeOnceAsync();
                }
                finally
                {
                    // 清理臨時檔案
                    if (System.IO.File.Exists(tempFileName))
                    {
                        System.IO.File.Delete(tempFileName);
                        _logger.LogInformation("臨時音檔已清理");
                    }
                }
                
                _logger.LogInformation($"單次識別結果: '{result.Text}', 原因: {result.Reason}");
                
                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    var confidence = string.IsNullOrEmpty(result.Text) ? 0.0 : 0.8;
                    var resultText = string.IsNullOrEmpty(result.Text) ? "無法識別語音內容" : result.Text;
                    
                    return new SpeechToTextResponse
                    {
                        Text = resultText,
                        Confidence = confidence,
                        Duration = duration
                    };
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    _logger.LogWarning("語音識別無匹配結果");
                    return new SpeechToTextResponse
                    {
                        Text = "無法識別語音內容 - 沒有找到匹配的語音",
                        Confidence = 0.0,
                        Duration = duration
                    };
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    _logger.LogError($"語音識別被取消: {cancellation.Reason}, 詳細: {cancellation.ErrorDetails}");
                    return new SpeechToTextResponse
                    {
                        Text = $"語音識別失敗: {cancellation.ErrorDetails}",
                        Confidence = 0.0,
                        Duration = duration
                    };
                }

                return new SpeechToTextResponse
                {
                    Text = "未知的語音識別結果",
                    Confidence = 0.0,
                    Duration = duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "語音識別過程中發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 上傳音檔到 Azure Blob Storage
        /// </summary>
        private async Task<string> UploadAudioToBlobStorage(IFormFile audioFile)
        {
            try
            {
                _logger.LogInformation("開始上傳音檔到 Azure Blob Storage...");
                
                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerName = _configuration["AzureStorage:ContainerName"];
                var audioFolder = _configuration["AzureStorage:AudioFolder"] ?? "case_audio/";

                _logger.LogInformation($"Azure Storage 設定檢查 - ConnectionString: {(string.IsNullOrEmpty(connectionString) ? "缺失" : "已設定")}, ContainerName: {containerName}, AudioFolder: {audioFolder}");

                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Azure Storage 連接字串未配置");
                    throw new InvalidOperationException("Azure Storage 連接字串未配置");
                }

                if (string.IsNullOrEmpty(containerName))
                {
                    _logger.LogError("Azure Storage 容器名稱未配置");
                    throw new InvalidOperationException("Azure Storage 容器名稱未配置");
                }

                _logger.LogInformation($"音檔資訊 - 檔案名: {audioFile.FileName}, 大小: {audioFile.Length} bytes, 內容類型: {audioFile.ContentType}");

                var blobServiceClient = new BlobServiceClient(connectionString);
                _logger.LogInformation("BlobServiceClient 建立成功");

                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                _logger.LogInformation($"容器客戶端建立成功，容器名稱: {containerName}");

                // 檢查容器是否存在，如果不存在則創建
                var containerExists = await containerClient.ExistsAsync();
                if (!containerExists.Value)
                {
                    _logger.LogInformation($"容器 {containerName} 不存在，正在創建...");
                    await containerClient.CreateIfNotExistsAsync();
                    _logger.LogInformation($"容器 {containerName} 創建成功");
                }
                else
                {
                    _logger.LogInformation($"容器 {containerName} 已存在");
                }

                // 生成唯一的檔案名稱
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
                var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
                var fileName = $"{audioFolder}{timestamp}_{guid}{extension}";
                
                _logger.LogInformation($"生成的檔案名稱: {fileName}");

                var blobClient = containerClient.GetBlobClient(fileName);
                _logger.LogInformation($"Blob 客戶端建立成功，完整路徑: {blobClient.Uri}");

                // 上傳音檔
                _logger.LogInformation("開始複製音檔到記憶體流...");
                using var stream = new MemoryStream();
                await audioFile.CopyToAsync(stream);
                stream.Position = 0;
                _logger.LogInformation($"音檔複製完成，流大小: {stream.Length} bytes");

                _logger.LogInformation("開始上傳到 Azure Blob Storage...");
                var uploadResult = await blobClient.UploadAsync(stream, overwrite: true);
                _logger.LogInformation($"上傳完成，ETag: {uploadResult.Value.ETag}");

                var finalUrl = blobClient.Uri.ToString();
                _logger.LogInformation($"音檔上傳成功，最終 URL: {finalUrl}");

                return finalUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "音檔上傳到 Blob Storage 失敗");
                throw; // 重新拋出異常，讓上層處理
            }
        }

        /// <summary>
        /// 從 JSON 結果中提取信心度
        /// </summary>
        private double GetConfidenceFromJson(string jsonResult)
        {
            try
            {
                // 簡單的 JSON 解析來取得信心度
                if (jsonResult.Contains("\"Confidence\":"))
                {
                    var confidenceStart = jsonResult.IndexOf("\"Confidence\":") + 13;
                    var confidenceEnd = jsonResult.IndexOf(",", confidenceStart);
                    if (confidenceEnd == -1)
                        confidenceEnd = jsonResult.IndexOf("}", confidenceStart);
                    
                    if (confidenceEnd > confidenceStart)
                    {
                        var confidenceStr = jsonResult.Substring(confidenceStart, confidenceEnd - confidenceStart).Trim();
                        if (double.TryParse(confidenceStr, out var confidence))
                        {
                            return confidence;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "解析信心度失敗");
            }

            return 0.8; // 預設信心度
        }
    }

    /// <summary>
    /// 語音轉文字回應模型
    /// </summary>
    public class SpeechToTextResponse
    {
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double Duration { get; set; }
        public string AudioUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// 音檔上傳回應模型
    /// </summary>
    public class AudioUploadResponse
    {
        public string AudioUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadTime { get; set; }
    }

    /// <summary>
    /// 從 URL 轉文字請求模型
    /// </summary>
    public class TranscribeFromUrlRequest
    {
        public string AudioUrl { get; set; } = string.Empty;
    }
} 