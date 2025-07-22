using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencySupplyNeedController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<EmergencySupplyNeedController> _logger;
        private readonly IConfiguration _configuration;

        public EmergencySupplyNeedController(
            NgoplatformDbContext context, 
            ILogger<EmergencySupplyNeedController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 獲取所有緊急物資需求
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmergencySupplyNeedResponse>>> GetAllEmergencySupplyNeeds()
        {
            try
            {
                var needs = await _context.EmergencySupplyNeeds
                    .Include(e => e.Case)
                    .Include(e => e.Worker)
                    .OrderByDescending(e => e.CreatedDate)
                    .ToListAsync();

                var response = needs.Select(need => new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = need.EmergencyNeedId,
                    CaseId = need.CaseId,
                    WorkerId = need.WorkerId,
                    SupplyName = need.SupplyName,
                    Quantity = need.Quantity,
                    CollectedQuantity = need.CollectedQuantity ?? 0,
                    Description = need.Description,
                    Priority = need.Priority,
                    Status = need.Status,
                    CreatedDate = need.CreatedDate,
                    UpdatedDate = need.UpdatedDate,
                    ImageUrl = need.ImageUrl,
                    CaseName = need.Case?.Name,
                    WorkerName = need.Worker?.Name
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取緊急物資需求列表失敗");
                return StatusCode(500, new { message = "獲取緊急物資需求列表失敗" });
            }
        }

        /// <summary>
        /// 根據 ID 獲取特定緊急物資需求
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmergencySupplyNeedResponse>> GetEmergencySupplyNeedById(int id)
        {
            try
            {
                var need = await _context.EmergencySupplyNeeds
                    .Include(e => e.Case)
                    .Include(e => e.Worker)
                    .FirstOrDefaultAsync(e => e.EmergencyNeedId == id);

                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的緊急物資需求" });
                }

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = need.EmergencyNeedId,
                    CaseId = need.CaseId,
                    WorkerId = need.WorkerId,
                    SupplyName = need.SupplyName,
                    Quantity = need.Quantity,
                    CollectedQuantity = need.CollectedQuantity ?? 0,
                    Description = need.Description,
                    Priority = need.Priority,
                    Status = need.Status,
                    CreatedDate = need.CreatedDate,
                    UpdatedDate = need.UpdatedDate,
                    ImageUrl = need.ImageUrl,
                    CaseName = need.Case?.Name,
                    WorkerName = need.Worker?.Name
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取緊急物資需求詳情失敗");
                return StatusCode(500, new { message = "獲取緊急物資需求詳情失敗" });
            }
        }

        /// <summary>
        /// 建立新的緊急物資需求
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EmergencySupplyNeedResponse>> CreateEmergencySupplyNeed(
            [FromBody] CreateEmergencySupplyNeedRequest request)
        {
            try
            {
                var newNeed = new EmergencySupplyNeed
                {
                    CaseId = request.CaseId,
                    WorkerId = request.WorkerId,
                    SupplyName = request.SupplyName,
                    Quantity = request.Quantity,
                    CollectedQuantity = 0,
                    Description = request.Description,
                    Priority = request.Priority ?? "Normal",
                    Status = "Fundraising",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    ImageUrl = request.ImageUrl
                };

                _context.EmergencySupplyNeeds.Add(newNeed);
                await _context.SaveChangesAsync();

                // 重新查詢以獲取完整資料
                var createdNeed = await _context.EmergencySupplyNeeds
                    .Include(e => e.Case)
                    .Include(e => e.Worker)
                    .FirstOrDefaultAsync(e => e.EmergencyNeedId == newNeed.EmergencyNeedId);

                var response = new EmergencySupplyNeedResponse
                {
                    EmergencyNeedId = createdNeed!.EmergencyNeedId,
                    CaseId = createdNeed.CaseId,
                    WorkerId = createdNeed.WorkerId,
                    SupplyName = createdNeed.SupplyName,
                    Quantity = createdNeed.Quantity,
                    CollectedQuantity = createdNeed.CollectedQuantity ?? 0,
                    Description = createdNeed.Description,
                    Priority = createdNeed.Priority,
                    Status = createdNeed.Status,
                    CreatedDate = createdNeed.CreatedDate,
                    UpdatedDate = createdNeed.UpdatedDate,
                    ImageUrl = createdNeed.ImageUrl,
                    CaseName = createdNeed.Case?.Name,
                    WorkerName = createdNeed.Worker?.Name
                };

                return CreatedAtAction(nameof(GetEmergencySupplyNeedById), 
                    new { id = newNeed.EmergencyNeedId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立緊急物資需求失敗");
                return StatusCode(500, new { message = "建立緊急物資需求失敗" });
            }
        }

        /// <summary>
        /// 更新緊急物資需求
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEmergencySupplyNeed(int id, 
            [FromBody] UpdateEmergencySupplyNeedRequest request)
        {
            try
            {
                var need = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的緊急物資需求" });
                }

                // 更新欄位
                if (request.SupplyName != null) need.SupplyName = request.SupplyName;
                if (request.Quantity.HasValue) need.Quantity = request.Quantity.Value;
                if (request.CollectedQuantity.HasValue) need.CollectedQuantity = request.CollectedQuantity.Value;
                if (request.Description != null) need.Description = request.Description;
                if (request.Priority != null) need.Priority = request.Priority;
                if (request.Status != null) need.Status = request.Status;
                if (request.ImageUrl != null) need.ImageUrl = request.ImageUrl;

                need.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新緊急物資需求失敗");
                return StatusCode(500, new { message = "更新緊急物資需求失敗" });
            }
        }

        /// <summary>
        /// 刪除緊急物資需求
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmergencySupplyNeed(int id)
        {
            try
            {
                var need = await _context.EmergencySupplyNeeds.FindAsync(id);
                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的緊急物資需求" });
                }

                _context.EmergencySupplyNeeds.Remove(need);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除緊急物資需求失敗");
                return StatusCode(500, new { message = "刪除緊急物資需求失敗" });
            }
        }

        /// <summary>
        /// 上傳緊急物資需求圖片
        /// </summary>
        [HttpPost("upload/image")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> UploadSupplyImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "請選擇圖片檔案" });
                }

                // 驗證檔案類型
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "只支援 JPG、PNG、GIF 格式的圖片" });
                }

                // 驗證檔案大小 (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "圖片檔案大小不能超過 5MB" });
                }

                // 從配置中獲取 Azure Storage 設定
                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerName = _configuration.GetValue<string>("AzureStorage:ContainerName");
                var supplyPhotosFolder = "emergency_supply_photos/";

                if (string.IsNullOrEmpty(connectionString))
                {
                    return StatusCode(500, new { message = "Azure Storage 連接字串未配置" });
                }

                // 建立 Azure Blob Service Client
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // 確保容器存在
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // 生成唯一的檔案名稱
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var blobName = $"{supplyPhotosFolder}{fileName}";

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
                
                _logger.LogInformation($"緊急物資需求圖片上傳成功: {fileName}, URL: {imageUrl}");

                return Ok(new { imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "緊急物資需求圖片上傳失敗");
                return StatusCode(500, new { message = "緊急物資需求圖片上傳失敗", error = ex.Message });
            }
        }
    }

    // Request/Response DTOs
    public class CreateEmergencySupplyNeedRequest
    {
        public int CaseId { get; set; }
        public int WorkerId { get; set; }
        public string SupplyName { get; set; } = null!;
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateEmergencySupplyNeedRequest
    {
        public string? SupplyName { get; set; }
        public int? Quantity { get; set; }
        public int? CollectedQuantity { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class EmergencySupplyNeedResponse
    {
        public int EmergencyNeedId { get; set; }
        public int CaseId { get; set; }
        public int WorkerId { get; set; }
        public string SupplyName { get; set; } = null!;
        public int Quantity { get; set; }
        public int CollectedQuantity { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? CaseName { get; set; }
        public string? WorkerName { get; set; }
    }
} 