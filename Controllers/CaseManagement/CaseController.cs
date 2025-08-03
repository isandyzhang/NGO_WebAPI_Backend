using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGO_WebAPI_Backend.Models.Shared;
using NGO_WebAPI_Backend.DTOs;
using NGO_WebAPI_Backend.Services;

namespace NGO_WebAPI_Backend.Controllers.CaseManagement
{
    /// <summary>
    /// 個案管理控制器
    /// 
    /// 使用三層架構設計：
    /// - Controller 層：處理 HTTP 請求和回應
    /// - Service 層：處理業務邏輯
    /// - Repository 層：處理資料存取
    /// - 統一的 ApiResponse 格式
    /// - DTO 物件進行資料傳輸
    /// </summary>
    [ApiController]
    [Route("api/case")]
    [Authorize]
    public class CaseController : ControllerBase
    {
        private readonly ICaseService _caseService;
        private readonly ILogger<CaseController> _logger;

        public CaseController(ICaseService caseService, ILogger<CaseController> logger)
        {
            _caseService = caseService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取個案列表（支援分頁和WorkerId過濾）
        /// HTTP GET: /api/casenew?page=1&pageSize=10&workerId=1
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedApiResponse<CaseDto>>> GetAllCases(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? workerId = null)
        {
            var response = await _caseService.GetAllCasesAsync(page, pageSize, workerId);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return StatusCode(500, response);
        }

        /// <summary>
        /// 根據 ID 獲取特定個案
        /// HTTP GET: /api/casenew/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CaseDto>>> GetCaseById(int id)
        {
            var response = await _caseService.GetCaseByIdAsync(id);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            if (response.Message.Contains("找不到"))
            {
                return NotFound(response);
            }
            
            return StatusCode(500, response);
        }

        /// <summary>
        /// 搜尋個案（支援分頁和WorkerId過濾）
        /// HTTP GET: /api/casenew/search?query=關鍵字&page=1&pageSize=10&workerId=1
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedApiResponse<CaseDto>>> SearchCases(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? workerId = null)
        {
            var response = await _caseService.SearchCasesAsync(query, page, pageSize, workerId);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return StatusCode(500, response);
        }

        /// <summary>
        /// 上傳個案圖片到 Azure Blob Storage
        /// HTTP POST: /api/casenew/upload/profile-image
        /// </summary>
        [HttpPost("upload/profile-image")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> UploadProfileImage(IFormFile file)
        {
            var response = await _caseService.UploadProfileImageAsync(file);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            if (response.Message.Contains("請選擇") || response.Message.Contains("只支援") || response.Message.Contains("不能超過"))
            {
                return BadRequest(response);
            }
            
            return StatusCode(500, response);
        }

        /// <summary>
        /// 建立新個案
        /// HTTP POST: /api/casenew
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CaseDto>>> CreateCase([FromBody] CreateCaseDto createCaseDto)
        {
            // 檢查模型驗證
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(ApiResponse<CaseDto>.ErrorResponse("輸入資料驗證失敗", new { 
                    ValidationErrors = errors,
                    Details = "請檢查輸入的資料格式是否正確"
                }));
            }

            var response = await _caseService.CreateCaseAsync(createCaseDto);
            
            if (response.Success)
            {
                return CreatedAtAction(nameof(GetCaseById), new { id = response.Data!.CaseId }, response);
            }
            
            if (response.Message.Contains("已存在") || response.Message.Contains("格式錯誤"))
            {
                return BadRequest(response);
            }
            
            return StatusCode(500, response);
        }

        /// <summary>
        /// 更新個案資料
        /// HTTP PUT: /api/casenew/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CaseDto>>> UpdateCase(int id, [FromBody] UpdateCaseDto updateCaseDto)
        {
            // 檢查模型驗證
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                return BadRequest(ApiResponse<CaseDto>.ErrorResponse("輸入資料驗證失敗", new { 
                    ValidationErrors = errors,
                    Details = "請檢查輸入的資料格式是否正確"
                }));
            }

            var response = await _caseService.UpdateCaseAsync(id, updateCaseDto);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            if (response.Message.Contains("找不到"))
            {
                return NotFound(response);
            }
            
            if (response.Message.Contains("格式錯誤") || response.Message.Contains("已被使用"))
            {
                return BadRequest(response);
            }
            
            return StatusCode(500, response);
        }

        /// <summary>
        /// 刪除個案
        /// HTTP DELETE: /api/casenew/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCase(int id)
        {
            var response = await _caseService.DeleteCaseAsync(id);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            if (response.Message.Contains("找不到"))
            {
                return NotFound(response);
            }
            
            if (response.Message.Contains("無法刪除"))
            {
                return BadRequest(response);
            }
            
            return StatusCode(500, response);
        }
    }
}