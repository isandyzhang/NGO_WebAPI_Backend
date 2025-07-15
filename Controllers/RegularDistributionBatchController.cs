using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegularDistributionBatchController : ControllerBase
{
    private readonly NgoplatformDbContext _context;

    public RegularDistributionBatchController(NgoplatformDbContext context)
    {
        _context = context;
    }

    // 获取所有分发批次
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetDistributionBatches()
    {
        try
        {
            var batches = await _context.RegularDistributionBatches
                .OrderByDescending(b => b.DistributionDate)
                .ToListAsync();

            var result = batches.Select(b => new
            {
                b.DistributionBatchId,
                b.DistributionDate,
                b.CaseCount,
                b.TotalSupplyItems,
                b.Status,
                b.CreatedAt,
                b.Notes,
                CreatedByWorker = "系統管理員"
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "無法獲取分發批次資料", detail = ex.Message });
        }
    }

    // 创建新的分发批次
    [HttpPost]
    public async Task<ActionResult<object>> CreateDistributionBatch([FromBody] CreateDistributionBatchRequest request)
    {
        try
        {
            var batch = new RegularDistributionBatch
            {
                DistributionDate = request.DistributionDate,
                CaseCount = request.CaseCount,
                TotalSupplyItems = request.TotalSupplyItems,
                CreatedByWorkerId = request.CreatedByWorkerId,
                Status = "pending",
                Notes = request.Notes
                // CreatedAt 會由資料庫自動設置預設值
            };

            _context.RegularDistributionBatches.Add(batch);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "分發批次創建成功", 
                id = batch.DistributionBatchId 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "無法創建分發批次", detail = ex.Message });
        }
    }

    // 删除分发批次
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDistributionBatch(int id)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches.FindAsync(id);
            if (batch == null)
            {
                return NotFound(new { error = "找不到指定的分發批次" });
            }

            _context.RegularDistributionBatches.Remove(batch);
            await _context.SaveChangesAsync();

            return Ok(new { message = "分發批次刪除成功" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "無法刪除分發批次", detail = ex.Message });
        }
    }

    // 測試端點
    [HttpGet("test")]
    public ActionResult<string> Test()
    {
        return Ok("RegularDistributionBatch Controller 正常運作!");
    }
}

// 請求模型
public class CreateDistributionBatchRequest
{
    public DateTime DistributionDate { get; set; }
    public int CaseCount { get; set; }
    public int TotalSupplyItems { get; set; }
    public int CreatedByWorkerId { get; set; }
    public string? Notes { get; set; }
} 