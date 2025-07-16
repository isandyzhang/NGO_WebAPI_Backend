using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegularSuppliesNeedController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public RegularSuppliesNeedController(NgoplatformDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 將資料庫中的中文狀態轉換為前端期望的英文狀態
        /// </summary>
        private static string ConvertStatusToEnglish(string status)
        {
            return status switch
            {
                "待審核" => "pending",
                "批准" => "approved",
                "approved" => "approved",
                "不批准" => "rejected",
                "rejected" => "rejected",
                "未領取" => "approved",
                "已領取" => "collected",
                "collected" => "collected",
                "completed" => "collected", // 向後兼容
                _ => "pending"
            };
        }

        /// <summary>
        /// 檢查狀態是否為待審核狀態
        /// </summary>
        private bool IsPendingStatus(string status)
        {
            return status == "pending" || status == "待審核";
        }

        /// <summary>
        /// 檢查狀態是否為批准狀態
        /// </summary>
        private bool IsApprovedStatus(string status)
        {
            return status == "approved" || status == "批准" || status == "未領取";
        }

        /// <summary>
        /// 檢查狀態是否為拒絕狀態
        /// </summary>
        private bool IsRejectedStatus(string status)
        {
            return status == "rejected" || status == "不批准";
        }

        // GET: api/RegularSuppliesNeed
        /// <summary>
        /// 取得所有常駐物資需求
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRegularSuppliesNeeds()
        {
            try
            {
                var needs = await _context.RegularSuppliesNeeds
                    .Include(r => r.Case)
                    .Include(r => r.Supply)
                    .ThenInclude(s => s!.SupplyCategory)
                    .Select(r => new
                    {
                        needId = r.RegularNeedId,
                        caseId = r.CaseId,
                        caseName = r.Case != null ? r.Case.Name : "未知",
                        supplyId = r.SupplyId,
                        itemName = r.Supply != null ? r.Supply.SupplyName : "未知",
                        category = r.Supply != null && r.Supply.SupplyCategory != null ? r.Supply.SupplyCategory.SupplyCategoryName : "未分類",
                        quantity = r.Quantity ?? 0,
                        unit = "個", // 暫時固定單位
                        requestedBy = r.Case != null ? r.Case.Name : "未知",
                        requestDate = r.ApplyDate != null ? r.ApplyDate.Value.ToString("yyyy-MM-dd") : "",
                        status = ConvertStatusToEnglish(r.Status ?? "pending"),
                        estimatedCost = r.Supply != null ? (r.Quantity ?? 0) * (r.Supply.SupplyPrice ?? 0) : 0,
                        deliveryMethod = "自取", // 暫時固定取件方式
                        pickupDate = r.PickupDate != null ? r.PickupDate.Value.ToString("yyyy-MM-dd") : null,
                        matched = false // 暫時固定為未媒合
                    })
                    .ToListAsync();

                return Ok(needs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得常駐物資需求失敗", error = ex.Message });
            }
        }

        // GET: api/RegularSuppliesNeed/5
        /// <summary>
        /// 取得單一常駐物資需求
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds
                    .Include(r => r.Case)
                    .Include(r => r.Supply)
                    .ThenInclude(s => s!.SupplyCategory)
                    .Where(r => r.RegularNeedId == id)
                    .Select(r => new
                    {
                        needId = r.RegularNeedId,
                        caseId = r.CaseId,
                        caseName = r.Case != null ? r.Case.Name : "未知",
                        supplyId = r.SupplyId,
                        itemName = r.Supply != null ? r.Supply.SupplyName : "未知",
                        category = r.Supply != null && r.Supply.SupplyCategory != null ? r.Supply.SupplyCategory.SupplyCategoryName : "未分類",
                        quantity = r.Quantity ?? 0,
                        unit = "個",
                        requestedBy = r.Case != null ? r.Case.Name : "未知",
                        requestDate = r.ApplyDate != null ? r.ApplyDate.Value.ToString("yyyy-MM-dd") : "",
                        status = r.Status ?? "pending",
                        estimatedCost = r.Supply != null ? (r.Quantity ?? 0) * (r.Supply.SupplyPrice ?? 0) : 0,
                        deliveryMethod = "自取",
                        pickupDate = r.PickupDate != null ? r.PickupDate.Value.ToString("yyyy-MM-dd") : null,
                        matched = false
                    })
                    .FirstOrDefaultAsync();

                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資需求" });
                }

                return Ok(need);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得常駐物資需求失敗", error = ex.Message });
            }
        }

        // POST: api/RegularSuppliesNeed
        /// <summary>
        /// 新增常駐物資需求
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> PostRegularSuppliesNeed([FromBody] CreateRegularSuppliesNeedRequest request)
        {
            try
            {
                var need = new RegularSuppliesNeed
                {
                    CaseId = request.CaseId,
                    SupplyId = request.SupplyId,
                    Quantity = request.Quantity,
                    ApplyDate = DateTime.Now,
                    Status = "pending"
                };

                _context.RegularSuppliesNeeds.Add(need);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRegularSuppliesNeed), new { id = need.RegularNeedId }, 
                    new { message = "常駐物資需求新增成功", needId = need.RegularNeedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "新增常駐物資需求失敗", error = ex.Message });
            }
        }

        // PUT: api/RegularSuppliesNeed/5
        /// <summary>
        /// 更新常駐物資需求
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegularSuppliesNeed(int id, [FromBody] UpdateRegularSuppliesNeedRequest request)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資需求" });
                }

                need.CaseId = request.CaseId ?? need.CaseId;
                need.SupplyId = request.SupplyId ?? need.SupplyId;
                need.Quantity = request.Quantity ?? need.Quantity;
                need.Status = request.Status ?? need.Status;
                need.PickupDate = request.PickupDate ?? need.PickupDate;

                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "常駐物資需求更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新常駐物資需求失敗", error = ex.Message });
            }
        }

        // DELETE: api/RegularSuppliesNeed/5
        /// <summary>
        /// 刪除常駐物資需求
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資需求" });
                }

                _context.RegularSuppliesNeeds.Remove(need);
                await _context.SaveChangesAsync();

                return Ok(new { message = "常駐物資需求刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "刪除常駐物資需求失敗", error = ex.Message });
            }
        }

        // POST: api/RegularSuppliesNeed/5/approve
        /// <summary>
        /// 批准常駐物資需求
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資需求" });
                }

                need.Status = "approved";
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "常駐物資需求批准成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "批准常駐物資需求失敗", error = ex.Message });
            }
        }

        // POST: api/RegularSuppliesNeed/5/reject
        /// <summary>
        /// 拒絕常駐物資需求
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRegularSuppliesNeed(int id)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資需求" });
                }

                need.Status = "rejected";
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "常駐物資需求拒絕成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "拒絕常駐物資需求失敗", error = ex.Message });
            }
        }

        // POST: api/RegularSuppliesNeed/5/collect
        /// <summary>
        /// 標記常駐物資需求為已領取
        /// </summary>
        [HttpPost("{id}/collect")]
        public async Task<IActionResult> CollectRegularSuppliesNeed(int id, [FromBody] CollectRegularSuppliesNeedRequest? request = null)
        {
            try
            {
                var need = await _context.RegularSuppliesNeeds.FindAsync(id);
                if (need == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資需求" });
                }

                need.Status = "collected";
                need.PickupDate = DateTime.Now; // 設定領取時間
                need.BatchId = request?.BatchId; // 設定批次ID
                _context.Entry(need).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "常駐物資需求標記為已領取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "標記常駐物資需求為已領取失敗", error = ex.Message });
            }
        }

        // GET: api/RegularSuppliesNeed/batch/5/details
        /// <summary>
        /// 根據批次ID取得分發詳情
        /// </summary>
        [HttpGet("batch/{batchId}/details")]
        public async Task<ActionResult<IEnumerable<object>>> GetBatchDistributionDetails(int batchId)
        {
            try
            {
                // 先查詢所有具有指定BatchId的記錄，不限制狀態，用於調試
                var allBatchRecords = await _context.RegularSuppliesNeeds
                    .Where(r => r.BatchId == batchId)
                    .Select(r => new { r.RegularNeedId, r.Status, r.BatchId })
                    .ToListAsync();

                Console.WriteLine($"查詢BatchId {batchId} 的記錄數: {allBatchRecords.Count}");
                foreach (var record in allBatchRecords)
                {
                    Console.WriteLine($"記錄ID: {record.RegularNeedId}, 狀態: '{record.Status}', BatchId: {record.BatchId}");
                }

                var batchDetails = await _context.RegularSuppliesNeeds
                    .Include(r => r.Case)
                    .Include(r => r.Supply)
                    .ThenInclude(s => s!.SupplyCategory)
                    .Where(r => r.BatchId == batchId)  // 暫時移除狀態限制來看所有記錄
                    .Select(r => new
                    {
                        needId = r.RegularNeedId,
                        申請人 = r.Case != null ? r.Case.Name : "未知",
                        物品名稱 = r.Supply != null ? r.Supply.SupplyName : "未知",
                        申請數量 = r.Quantity ?? 0,
                        配對數量 = r.Quantity ?? 0,
                        申請日期 = r.ApplyDate != null ? r.ApplyDate.Value.ToString("yyyy/M/d") : "",
                        配對日期 = r.PickupDate != null ? r.PickupDate.Value.ToString("yyyy/M/d") : "",
                        狀態 = r.Status ?? "未知",
                        備註 = "系統管理員"
                    })
                    .ToListAsync();

                Console.WriteLine($"返回的詳情記錄數: {batchDetails.Count}");
                return Ok(batchDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查詢批次詳情時發生錯誤: {ex.Message}");
                return StatusCode(500, new { message = "取得批次分發詳情失敗", error = ex.Message });
            }
        }

        // GET: api/RegularSuppliesNeed/stats
        /// <summary>
        /// 取得常駐物資需求統計
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetRegularSuppliesNeedStats()
        {
            try
            {
                var totalRequests = await _context.RegularSuppliesNeeds.CountAsync();
                
                var allRequests = await _context.RegularSuppliesNeeds
                    .Select(r => r.Status)
                    .ToListAsync();
                
                var pendingRequests = allRequests.Count(status => IsPendingStatus(status ?? ""));
                var approvedRequests = allRequests.Count(status => IsApprovedStatus(status ?? ""));
                var rejectedRequests = allRequests.Count(status => IsRejectedStatus(status ?? ""));

                var totalEstimatedCost = await _context.RegularSuppliesNeeds
                    .Include(r => r.Supply)
                    .SumAsync(r => (r.Quantity ?? 0) * (r.Supply != null ? r.Supply.SupplyPrice ?? 0 : 0));

                var stats = new
                {
                    totalRequests = totalRequests,
                    pendingRequests = pendingRequests,
                    approvedRequests = approvedRequests,
                    rejectedRequests = rejectedRequests,
                    totalEstimatedCost = totalEstimatedCost
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得常駐物資需求統計失敗", error = ex.Message });
            }
        }
    }

    public class CreateRegularSuppliesNeedRequest
    {
        public int CaseId { get; set; }
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateRegularSuppliesNeedRequest
    {
        public int? CaseId { get; set; }
        public int? SupplyId { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
        public DateTime? PickupDate { get; set; }
    }

    public class CollectRegularSuppliesNeedRequest
    {
        public int? BatchId { get; set; }
    }
} 