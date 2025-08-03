using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NGO_WebAPI_Backend.Models.Shared;
using NGO_WebAPI_Backend.Models.Domain.CaseManagement;
using NGO_WebAPI_Backend.DTOs;
using NGO_WebAPI_Backend.Repositories;
using System.Text.RegularExpressions;

namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// 個案服務實作
    /// </summary>
    public class CaseService : ICaseService
    {
        private readonly ICaseRepository _caseRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CaseService> _logger;

        public CaseService(ICaseRepository caseRepository, IConfiguration configuration, ILogger<CaseService> logger)
        {
            _caseRepository = caseRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有個案（支援分頁和過濾）
        /// </summary>
        public async Task<PagedApiResponse<CaseDto>> GetAllCasesAsync(int page, int pageSize, int? workerId = null)
        {
            try
            {
                _logger.LogInformation($"開始獲取個案列表，頁碼: {page}, 每頁數量: {pageSize}, WorkerId: {workerId}");

                // 驗證分頁參數
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (cases, totalCount) = await _caseRepository.GetAllCasesAsync(page, pageSize, workerId);
                var caseDtos = cases.Select(MapToCaseDto).ToList();

                _logger.LogInformation($"成功獲取個案列表，共 {totalCount} 筆");
                return PagedApiResponse<CaseDto>.SuccessResponse(caseDtos, page, pageSize, totalCount, "獲取個案列表成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個案列表時發生錯誤");
                return new PagedApiResponse<CaseDto>
                {
                    Success = false,
                    Message = "獲取個案列表失敗",
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 根據 ID 取得個案
        /// </summary>
        public async Task<ApiResponse<CaseDto>> GetCaseByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"開始獲取個案 ID: {id}");

                var caseEntity = await _caseRepository.GetCaseByIdAsync(id);
                if (caseEntity == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return ApiResponse<CaseDto>.ErrorResponse("找不到指定的個案");
                }

                var caseDto = MapToCaseDto(caseEntity);
                _logger.LogInformation($"成功獲取個案 ID: {id}");
                return ApiResponse<CaseDto>.SuccessResponse(caseDto, "獲取個案詳情成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取個案 ID: {id} 時發生錯誤");
                return ApiResponse<CaseDto>.ErrorResponse("獲取個案詳情失敗", ex.Message);
            }
        }

        /// <summary>
        /// 搜尋個案
        /// </summary>
        public async Task<PagedApiResponse<CaseDto>> SearchCasesAsync(string? query, int page, int pageSize, int? workerId = null)
        {
            try
            {
                _logger.LogInformation($"開始搜尋個案，關鍵字: {query}, 頁碼: {page}, 每頁數量: {pageSize}, WorkerId: {workerId}");

                // 驗證分頁參數
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (cases, totalCount) = await _caseRepository.SearchCasesAsync(query, page, pageSize, workerId);
                var caseDtos = cases.Select(MapToCaseDto).ToList();

                _logger.LogInformation($"搜尋完成，找到 {totalCount} 個個案");
                return PagedApiResponse<CaseDto>.SuccessResponse(caseDtos, page, pageSize, totalCount, "搜尋個案成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋個案時發生錯誤");
                return new PagedApiResponse<CaseDto>
                {
                    Success = false,
                    Message = "搜尋個案失敗",
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 建立個案
        /// </summary>
        public async Task<ApiResponse<CaseDto>> CreateCaseAsync(CreateCaseDto createCaseDto)
        {
            try
            {
                _logger.LogInformation($"開始建立新個案，姓名: {createCaseDto.Name}");

                // FluentValidation 已經會驗證身分證字號格式，這裡不需要重複驗證

                // 檢查身分證字號是否已存在
                var isExists = await _caseRepository.IsIdentityNumberExistsAsync(createCaseDto.IdentityNumber);
                if (isExists)
                {
                    var existingCase = await _caseRepository.GetCaseByIdentityNumberAsync(createCaseDto.IdentityNumber);
                    _logger.LogWarning($"身分證字號 {createCaseDto.IdentityNumber} 已存在");
                    return ApiResponse<CaseDto>.ErrorResponse("此身分證字號已存在", new 
                    { 
                        errorType = "DUPLICATE_IDENTITY",
                        existingCaseId = existingCase?.CaseId,
                        existingCaseName = existingCase?.Name,
                        existingCaseCreatedAt = existingCase?.CreatedAt,
                        suggestion = "請使用查詢功能搜尋該個案"
                    });
                }

                // 建立個案實體
                var caseEntity = new Case
                {
                    Name = createCaseDto.Name,
                    Phone = createCaseDto.Phone,
                    IdentityNumber = createCaseDto.IdentityNumber,
                    Birthday = createCaseDto.Birthday.HasValue ? DateOnly.FromDateTime(createCaseDto.Birthday.Value) : null,
                    WorkerId = createCaseDto.WorkerId ?? 1,
                    Description = createCaseDto.Description,
                    CreatedAt = DateTime.Now,
                    Status = "active",
                    Email = createCaseDto.Email,
                    Gender = createCaseDto.Gender,
                    ProfileImage = createCaseDto.ProfileImage,
                    City = createCaseDto.City,
                    District = createCaseDto.District,
                    DetailAddress = createCaseDto.DetailAddress,
                    SpeechToTextAudioUrl = createCaseDto.SpeechToTextAudioUrl
                };

                var createdCase = await _caseRepository.CreateCaseAsync(caseEntity);
                var caseDto = MapToCaseDto(createdCase);

                _logger.LogInformation($"成功建立個案 ID: {createdCase.CaseId}");
                return ApiResponse<CaseDto>.SuccessResponse(caseDto, "建立個案成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立個案時發生錯誤");
                return ApiResponse<CaseDto>.ErrorResponse("建立個案失敗", ex.Message);
            }
        }

        /// <summary>
        /// 更新個案
        /// </summary>
        public async Task<ApiResponse<CaseDto>> UpdateCaseAsync(int id, UpdateCaseDto updateCaseDto)
        {
            try
            {
                _logger.LogInformation($"開始更新個案 ID: {id}");

                var caseEntity = await _caseRepository.GetCaseByIdAsync(id);
                if (caseEntity == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return ApiResponse<CaseDto>.ErrorResponse("找不到指定的個案");
                }

                // 檢查身分證字號是否已被其他個案使用（如果有更新）
                if (!string.IsNullOrEmpty(updateCaseDto.IdentityNumber))
                {
                    var isExists = await _caseRepository.IsIdentityNumberExistsAsync(updateCaseDto.IdentityNumber, id);
                    if (isExists)
                    {
                        _logger.LogWarning($"身分證字號 {updateCaseDto.IdentityNumber} 已被其他個案使用");
                        return ApiResponse<CaseDto>.ErrorResponse("此身分證字號已被其他個案使用");
                    }
                }

                // 更新欄位
                if (updateCaseDto.Name != null) caseEntity.Name = updateCaseDto.Name;
                if (updateCaseDto.Phone != null) caseEntity.Phone = updateCaseDto.Phone;
                if (updateCaseDto.IdentityNumber != null) caseEntity.IdentityNumber = updateCaseDto.IdentityNumber;
                if (updateCaseDto.Birthday.HasValue) caseEntity.Birthday = DateOnly.FromDateTime(updateCaseDto.Birthday.Value);
                if (updateCaseDto.Description != null) caseEntity.Description = updateCaseDto.Description;
                if (updateCaseDto.Status != null) caseEntity.Status = updateCaseDto.Status;
                if (updateCaseDto.Email != null) caseEntity.Email = updateCaseDto.Email;
                if (updateCaseDto.Gender != null) caseEntity.Gender = updateCaseDto.Gender;
                if (updateCaseDto.ProfileImage != null) caseEntity.ProfileImage = updateCaseDto.ProfileImage;
                if (updateCaseDto.City != null) caseEntity.City = updateCaseDto.City;
                if (updateCaseDto.District != null) caseEntity.District = updateCaseDto.District;
                if (updateCaseDto.DetailAddress != null) caseEntity.DetailAddress = updateCaseDto.DetailAddress;

                var updatedCase = await _caseRepository.UpdateCaseAsync(caseEntity);
                var caseDto = MapToCaseDto(updatedCase);

                _logger.LogInformation($"成功更新個案 ID: {id}");
                return ApiResponse<CaseDto>.SuccessResponse(caseDto, "更新個案成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新個案 ID: {id} 時發生錯誤");
                return ApiResponse<CaseDto>.ErrorResponse("更新個案失敗", ex.Message);
            }
        }

        /// <summary>
        /// 刪除個案
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteCaseAsync(int id)
        {
            try
            {
                _logger.LogInformation($"開始刪除個案 ID: {id}");

                var caseEntity = await _caseRepository.GetCaseByIdAsync(id);
                if (caseEntity == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return ApiResponse<bool>.ErrorResponse("找不到指定的個案");
                }

                // 檢查是否有相關資料
                var relatedData = await _caseRepository.GetCaseRelatedDataAsync(id);
                if (relatedData.Count > 0)
                {
                    var errorMessage = $"無法刪除個案，因為該個案還有以下相關資料：\n{string.Join("\n", relatedData)}\n\n請先刪除這些相關資料後再刪除個案。";
                    _logger.LogWarning($"個案 ID: {id} 有相關資料，無法刪除。相關資料：{string.Join(", ", relatedData)}");
                    return ApiResponse<bool>.ErrorResponse("無法刪除個案", new
                    {
                        details = errorMessage,
                        relatedData = relatedData,
                        message = "無法刪除個案"
                    });
                }

                var result = await _caseRepository.DeleteCaseAsync(id);
                if (result)
                {
                    _logger.LogInformation($"成功刪除個案 ID: {id}");
                    return ApiResponse<bool>.SuccessResponse(true, "刪除個案成功");
                }
                else
                {
                    _logger.LogWarning($"刪除個案 ID: {id} 失敗");
                    return ApiResponse<bool>.ErrorResponse("刪除個案失敗");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除個案 ID: {id} 時發生錯誤");
                return ApiResponse<bool>.ErrorResponse("刪除個案失敗", ex.Message);
            }
        }

        /// <summary>
        /// 上傳個案圖片
        /// </summary>
        public async Task<ApiResponse<string>> UploadProfileImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return ApiResponse<string>.ErrorResponse("請選擇圖片檔案");
                }

                // 驗證檔案類型
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return ApiResponse<string>.ErrorResponse("只支援 JPG、PNG、GIF 格式的圖片");
                }

                // 驗證檔案大小 (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return ApiResponse<string>.ErrorResponse("圖片檔案大小不能超過 5MB");
                }

                // 從配置中獲取 Azure Storage 設定
                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerName = _configuration.GetValue<string>("AzureStorage:ContainerName");
                var casePhotosFolder = _configuration.GetValue<string>("AzureStorage:CasePhotosFolder");

                if (string.IsNullOrEmpty(connectionString))
                {
                    return ApiResponse<string>.ErrorResponse("Azure Storage 連接字串未配置");
                }

                // 建立 Azure Blob Service Client
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // 確保容器存在
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // 生成唯一的檔案名稱
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var blobName = $"{casePhotosFolder}{fileName}";

                // 獲取 Blob Client
                var blobClient = containerClient.GetBlobClient(blobName);

                // 設定 Blob 的 Content-Type
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                };

                // 上傳檔案到 Azure Blob Storage
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders
                    });
                }

                // 回傳 Azure Blob URL
                var imageUrl = blobClient.Uri.ToString();
                
                _logger.LogInformation($"個案圖片上傳成功: {fileName}, URL: {imageUrl}");
                return ApiResponse<string>.SuccessResponse(imageUrl, "圖片上傳成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "個案圖片上傳失敗");
                return ApiResponse<string>.ErrorResponse("個案圖片上傳失敗", ex.Message);
            }
        }

        /// <summary>
        /// 將 Case 實體轉換為 CaseDto
        /// </summary>
        private static CaseDto MapToCaseDto(Case caseEntity)
        {
            return new CaseDto
            {
                CaseId = caseEntity.CaseId,
                Name = caseEntity.Name ?? string.Empty,
                Phone = caseEntity.Phone ?? string.Empty,
                IdentityNumber = caseEntity.IdentityNumber ?? string.Empty,
                Birthday = caseEntity.Birthday?.ToDateTime(TimeOnly.MinValue),
                Address = $"{caseEntity.City ?? ""}{caseEntity.District ?? ""}{caseEntity.DetailAddress ?? ""}",
                WorkerId = caseEntity.WorkerId ?? 0,
                Description = caseEntity.Description,
                CreatedAt = caseEntity.CreatedAt ?? DateTime.Now,
                Status = caseEntity.Status ?? string.Empty,
                Email = caseEntity.Email,
                Gender = caseEntity.Gender,
                ProfileImage = caseEntity.ProfileImage,
                City = caseEntity.City,
                District = caseEntity.District,
                DetailAddress = caseEntity.DetailAddress,
                WorkerName = caseEntity.Worker?.Name,
                SpeechToTextAudioUrl = caseEntity.SpeechToTextAudioUrl
            };
        }

        /// <summary>
        /// 驗證台灣身分證字號格式
        /// </summary>
        private static (bool IsValid, string ErrorMessage) ValidateTaiwanIdentityNumber(string identityNumber)
        {
            // 檢查是否為空
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return (false, "身分證字號不能為空");
            }

            // 檢查長度
            if (identityNumber.Length != 10)
            {
                return (false, "身分證字號必須為10位數字");
            }

            // 檢查格式：第一個字母 + 9個數字
            if (!Regex.IsMatch(identityNumber, @"^[A-Z][0-9]{9}$"))
            {
                return (false, "身分證字號格式錯誤：應為1個英文字母後接9個數字");
            }

            // 台灣身分證字號驗證規則
            var letterValues = new Dictionary<char, int>
            {
                {'A', 10}, {'B', 11}, {'C', 12}, {'D', 13}, {'E', 14},
                {'F', 15}, {'G', 16}, {'H', 17}, {'I', 34}, {'J', 18},
                {'K', 19}, {'L', 20}, {'M', 21}, {'N', 22}, {'O', 35},
                {'P', 23}, {'Q', 24}, {'R', 25}, {'S', 26}, {'T', 27},
                {'U', 28}, {'V', 29}, {'W', 32}, {'X', 30}, {'Y', 31}, {'Z', 33}
            };

            char firstLetter = identityNumber[0];
            if (!letterValues.ContainsKey(firstLetter))
            {
                return (false, "身分證字號第一個字母無效");
            }

            // 取得字母對應的數字
            int letterValue = letterValues[firstLetter];
            
            // 計算驗證碼
            int sum = (letterValue / 10) + (letterValue % 10) * 9;
            
            // 加上後9位數字的權重
            for (int i = 1; i < 9; i++)
            {
                int digit = int.Parse(identityNumber[i].ToString());
                sum += digit * (9 - i);
            }
            
            // 加上最後一位數字
            int lastDigit = int.Parse(identityNumber[9].ToString());
            sum += lastDigit;

            // 檢查是否能被10整除
            if (sum % 10 != 0)
            {
                return (false, "身分證字號驗證碼錯誤");
            }

            return (true, string.Empty);
        }
    }
}