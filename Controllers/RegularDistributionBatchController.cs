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
                distributionBatchId = b.DistributionBatchId,
                distributionDate = b.DistributionDate.ToString("yyyy-MM-dd"),
                caseCount = b.CaseCount,
                totalSupplyItems = b.TotalSupplyItems,
                status = b.Status ?? "pending",
                createdAt = b.CreatedAt.HasValue ? b.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                approvedAt = b.ApprovedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                notes = b.Notes,
                createdByWorker = "系統管理員",
                approvedByWorker = b.ApprovedAt != null ? "系統管理員" : null,
                matchCount = 0 // 暫時固定，後續可加入實際配對數量計算
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "無法獲取分發批次資料", detail = ex.Message });
        }
    }

    // 获取单个分发批次详情
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetDistributionBatch(int id)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches
                .Where(b => b.DistributionBatchId == id)
                .FirstOrDefaultAsync();

            if (batch == null)
            {
                return NotFound(new { error = "找不到指定的分發批次" });
            }

            var result = new
            {
                distributionBatchId = batch.DistributionBatchId,
                distributionDate = batch.DistributionDate.ToString("yyyy-MM-dd"),
                caseCount = batch.CaseCount,
                totalSupplyItems = batch.TotalSupplyItems,
                status = batch.Status ?? "pending",
                createdAt = batch.CreatedAt.HasValue ? batch.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                approvedAt = batch.ApprovedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                notes = batch.Notes,
                createdByWorker = "系統管理員",
                approvedByWorker = batch.ApprovedAt != null ? "系統管理員" : null,
                matchCount = 0,
                matches = new List<object>() // 空的配對清單，後續可擴展
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "無法獲取分發批次詳情", detail = ex.Message });
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

    // 批准分发批次
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<object>> ApproveDistributionBatch(int id, [FromBody] ApproveDistributionBatchRequest request)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches.FindAsync(id);
            if (batch == null)
            {
                return NotFound(new { error = "找不到指定的分發批次" });
            }

            batch.Status = "approved";
            batch.ApprovedAt = DateTime.Now;
            batch.ApprovedByWorkerId = request.ApprovedByWorkerId; // 使用請求中的 ID
            
            _context.Entry(batch).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "分發批次批准成功" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "無法批准分發批次", detail = ex.Message });
        }
    }

    // 拒絕分發批次
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<object>> RejectDistributionBatch(int id, [FromBody] RejectDistributionBatchRequest request)
    {
        try
        {
            var batch = await _context.RegularDistributionBatches.FindAsync(id);
            if (batch == null)
            {
                return NotFound(new { error = "找不到指定的分發批次" });
            }

            batch.Status = "rejected";
            batch.ApprovedAt = DateTime.Now;
            batch.ApprovedByWorkerId = request.RejectedByWorkerId; // 使用請求中的 ID
            batch.Notes = request.RejectReason ?? "主管拒絕";
            
            _context.Entry(batch).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "分發批次已拒絕" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "無法拒絕分發批次", detail = ex.Message });
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
    public int[]? MatchIds { get; set; }
}

public class ApproveDistributionBatchRequest
{
    public int ApprovedByWorkerId { get; set; }
}

public class RejectDistributionBatchRequest
{
    public int RejectedByWorkerId { get; set; }
    public string? RejectReason { get; set; }
} 