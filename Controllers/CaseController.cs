using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;
using NGO_WebAPI_Backend.Data;
using System.Linq;

namespace NGO_WebAPI_Backend.Controllers
{
    /// <summary>
    /// 個案管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CaseController> _logger;

        public CaseController(ApplicationDbContext context, ILogger<CaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 獲取所有個案
        /// </summary>
        /// <returns>個案列表</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CaseResponse>>> GetAllCases()
        {
            try
            {
                _logger.LogInformation("開始獲取所有個案");

                var cases = await _context.Cases
                    .Include(c => c.Worker)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CaseResponse
                    {
                        CaseId = c.CaseId,
                        Name = c.Name,
                        Phone = c.Phone,
                        IdentityNumber = c.IdentityNumber,
                        Birthday = c.Birthday,
                        Address = c.Address,
                        WorkerId = c.WorkerId,
                        Description = c.Description,
                        CreatedAt = c.CreatedAt,
                        Status = c.Status,
                        Email = c.Email,
                        Gender = c.Gender,
                        ProfileImage = c.ProfileImage,
                        City = c.City,
                        District = c.District,
                        DetailAddress = c.DetailAddress,
                        WorkerName = c.Worker != null ? c.Worker.Name : null
                    })
                    .ToListAsync();

                _logger.LogInformation($"成功獲取 {cases.Count} 個個案");
                return Ok(cases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個案列表時發生錯誤");
                return StatusCode(500, new { message = "獲取個案列表失敗" });
            }
        }

        /// <summary>
        /// 根據ID獲取個案
        /// </summary>
        /// <param name="id">個案ID</param>
        /// <returns>個案詳情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CaseResponse>> GetCaseById(int id)
        {
            try
            {
                _logger.LogInformation($"開始獲取個案 ID: {id}");

                var caseItem = await _context.Cases
                    .Include(c => c.Worker)
                    .FirstOrDefaultAsync(c => c.CaseId == id);

                if (caseItem == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return NotFound(new { message = "找不到指定的個案" });
                }

                var response = new CaseResponse
                {
                    CaseId = caseItem.CaseId,
                    Name = caseItem.Name,
                    Phone = caseItem.Phone,
                    IdentityNumber = caseItem.IdentityNumber,
                    Birthday = caseItem.Birthday,
                    Address = caseItem.Address,
                    WorkerId = caseItem.WorkerId,
                    Description = caseItem.Description,
                    CreatedAt = caseItem.CreatedAt,
                    Status = caseItem.Status,
                    Email = caseItem.Email,
                    Gender = caseItem.Gender,
                    ProfileImage = caseItem.ProfileImage,
                    City = caseItem.City,
                    District = caseItem.District,
                    DetailAddress = caseItem.DetailAddress,
                    WorkerName = caseItem.Worker?.Name
                };

                _logger.LogInformation($"成功獲取個案 ID: {id}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取個案 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "獲取個案詳情失敗" });
            }
        }

        /// <summary>
        /// 搜尋個案
        /// </summary>
        /// <param name="query">搜尋關鍵字</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns>搜尋結果</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CaseResponse>>> SearchCases(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"開始搜尋個案，關鍵字: {query}, 頁碼: {page}, 每頁數量: {pageSize}");

                var queryable = _context.Cases
                    .Include(c => c.Worker)
                    .AsQueryable();

                // 如果有搜尋關鍵字，進行模糊搜尋
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryable = queryable.Where(c =>
                        c.Name.Contains(query) ||
                        c.Phone.Contains(query) ||
                        c.IdentityNumber.Contains(query) ||
                        c.Email.Contains(query) ||
                        c.Description.Contains(query) ||
                        c.City.Contains(query) ||
                        c.District.Contains(query)
                    );
                }

                var totalCount = await queryable.CountAsync();
                var cases = await queryable
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CaseResponse
                    {
                        CaseId = c.CaseId,
                        Name = c.Name,
                        Phone = c.Phone,
                        IdentityNumber = c.IdentityNumber,
                        Birthday = c.Birthday,
                        Address = c.Address,
                        WorkerId = c.WorkerId,
                        Description = c.Description,
                        CreatedAt = c.CreatedAt,
                        Status = c.Status,
                        Email = c.Email,
                        Gender = c.Gender,
                        ProfileImage = c.ProfileImage,
                        City = c.City,
                        District = c.District,
                        DetailAddress = c.DetailAddress,
                        WorkerName = c.Worker != null ? c.Worker.Name : null
                    })
                    .ToListAsync();

                _logger.LogInformation($"搜尋完成，找到 {cases.Count} 個個案，總計 {totalCount} 個");

                return Ok(new
                {
                    data = cases,
                    total = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋個案時發生錯誤");
                return StatusCode(500, new { message = "搜尋個案失敗" });
            }
        }

        /// <summary>
        /// 建立新個案
        /// </summary>
        /// <param name="request">建立個案請求</param>
        /// <returns>建立的個案</returns>
        [HttpPost]
        public async Task<ActionResult<CaseResponse>> CreateCase([FromBody] CreateCaseRequest request)
        {
            try
            {
                _logger.LogInformation($"開始建立新個案，姓名: {request.Name}");

                // 檢查身分證字號是否已存在
                var existingCase = await _context.Cases
                    .FirstOrDefaultAsync(c => c.IdentityNumber == request.IdentityNumber);

                if (existingCase != null)
                {
                    _logger.LogWarning($"身分證字號 {request.IdentityNumber} 已存在");
                    return BadRequest(new { message = "此身分證字號已存在" });
                }

                // 建立新個案
                var newCase = new Case
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    IdentityNumber = request.IdentityNumber,
                    Birthday = request.Birthday,
                    Address = request.Address,
                    WorkerId = 1, // 預設分配給第一個工作人員
                    Description = request.Description,
                    CreatedAt = DateTime.Now,
                    Status = "Active",
                    Email = request.Email,
                    Gender = request.Gender,
                    ProfileImage = request.ProfileImage,
                    City = request.City,
                    District = request.District,
                    DetailAddress = request.DetailAddress
                };

                _context.Cases.Add(newCase);
                await _context.SaveChangesAsync();

                // 重新查詢以獲取完整資料
                var createdCase = await _context.Cases
                    .Include(c => c.Worker)
                    .FirstOrDefaultAsync(c => c.CaseId == newCase.CaseId);

                var response = new CaseResponse
                {
                    CaseId = createdCase!.CaseId,
                    Name = createdCase.Name,
                    Phone = createdCase.Phone,
                    IdentityNumber = createdCase.IdentityNumber,
                    Birthday = createdCase.Birthday,
                    Address = createdCase.Address,
                    WorkerId = createdCase.WorkerId,
                    Description = createdCase.Description,
                    CreatedAt = createdCase.CreatedAt,
                    Status = createdCase.Status,
                    Email = createdCase.Email,
                    Gender = createdCase.Gender,
                    ProfileImage = createdCase.ProfileImage,
                    City = createdCase.City,
                    District = createdCase.District,
                    DetailAddress = createdCase.DetailAddress,
                    WorkerName = createdCase.Worker?.Name
                };

                _logger.LogInformation($"成功建立個案 ID: {newCase.CaseId}");
                return CreatedAtAction(nameof(GetCaseById), new { id = newCase.CaseId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立個案時發生錯誤");
                return StatusCode(500, new { message = "建立個案失敗" });
            }
        }

        /// <summary>
        /// 更新個案
        /// </summary>
        /// <param name="id">個案ID</param>
        /// <param name="request">更新個案請求</param>
        /// <returns>更新結果</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCase(int id, [FromBody] UpdateCaseRequest request)
        {
            try
            {
                _logger.LogInformation($"開始更新個案 ID: {id}");

                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return NotFound(new { message = "找不到指定的個案" });
                }

                // 更新欄位
                if (request.Name != null) caseItem.Name = request.Name;
                if (request.Phone != null) caseItem.Phone = request.Phone;
                if (request.IdentityNumber != null) caseItem.IdentityNumber = request.IdentityNumber;
                if (request.Birthday.HasValue) caseItem.Birthday = request.Birthday;
                if (request.Address != null) caseItem.Address = request.Address;
                if (request.Description != null) caseItem.Description = request.Description;
                if (request.Status != null) caseItem.Status = request.Status;
                if (request.Email != null) caseItem.Email = request.Email;
                if (request.Gender != null) caseItem.Gender = request.Gender;
                if (request.ProfileImage != null) caseItem.ProfileImage = request.ProfileImage;
                if (request.City != null) caseItem.City = request.City;
                if (request.District != null) caseItem.District = request.District;
                if (request.DetailAddress != null) caseItem.DetailAddress = request.DetailAddress;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功更新個案 ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新個案 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "更新個案失敗" });
            }
        }

        /// <summary>
        /// 刪除個案
        /// </summary>
        /// <param name="id">個案ID</param>
        /// <returns>刪除結果</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCase(int id)
        {
            try
            {
                _logger.LogInformation($"開始刪除個案 ID: {id}");

                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null)
                {
                    _logger.LogWarning($"找不到個案 ID: {id}");
                    return NotFound(new { message = "找不到指定的個案" });
                }

                _context.Cases.Remove(caseItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"成功刪除個案 ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除個案 ID: {id} 時發生錯誤");
                return StatusCode(500, new { message = "刪除個案失敗" });
            }
        }

        /// <summary>
        /// 測試端點 - 檢查資料庫連接
        /// </summary>
        /// <returns>測試結果</returns>
        [HttpGet("test")]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("開始測試個案資料庫連接");

                var count = await _context.Cases.CountAsync();
                var sampleCase = await _context.Cases.FirstOrDefaultAsync();

                _logger.LogInformation($"資料庫連接成功，個案總數: {count}");

                return Ok(new
                {
                    message = "個案資料庫連接成功",
                    totalCases = count,
                    sampleCase = sampleCase != null ? new
                    {
                        id = sampleCase.CaseId,
                        name = sampleCase.Name,
                        phone = sampleCase.Phone
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試個案資料庫連接時發生錯誤");
                return StatusCode(500, new { message = "個案資料庫連接失敗", error = ex.Message });
            }
        }
    }
} 