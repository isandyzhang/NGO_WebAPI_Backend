using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers.CaseManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaseOrderController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public CaseOrderController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/CaseOrder
        /// <summary>
        /// 取得所有個案訂單
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCaseOrders()
        {
            try
            {
                var orders = await _context.CaseOrders
                    .Include(o => o.Case)
                    .Include(o => o.Supply)
                    .Select(o => new
                    {
                        caseOrderId = o.CaseOrderId,
                        caseId = o.CaseId,
                        caseName = o.Case != null ? o.Case.Name : "未知個案",
                        supplyId = o.SupplyId,
                        supplyName = o.Supply != null ? o.Supply.SupplyName : "未知物資",
                        quantity = o.Quantity ?? 0,
                        orderTime = o.OrderTime != null ? o.OrderTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        status = "pending" // CaseOrder模型沒有Status欄位，使用固定值
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得個案訂單失敗", error = ex.Message });
            }
        }

        // GET: api/CaseOrder/5
        /// <summary>
        /// 取得單一個案訂單
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCaseOrder(int id)
        {
            try
            {
                var order = await _context.CaseOrders
                    .Include(o => o.Case)
                    .Include(o => o.Supply)
                    .Where(o => o.CaseOrderId == id)
                    .Select(o => new
                    {
                        caseOrderId = o.CaseOrderId,
                        caseId = o.CaseId,
                        caseName = o.Case != null ? o.Case.Name : "未知個案",
                        supplyId = o.SupplyId,
                        supplyName = o.Supply != null ? o.Supply.SupplyName : "未知物資",
                        quantity = o.Quantity ?? 0,
                        orderTime = o.OrderTime != null ? o.OrderTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        status = "pending" // CaseOrder模型沒有Status欄位，使用固定值
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new { message = "找不到指定的個案訂單" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得個案訂單失敗", error = ex.Message });
            }
        }

        // POST: api/CaseOrder
        /// <summary>
        /// 新增個案訂單
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> PostCaseOrder([FromBody] CreateCaseOrderRequest request)
        {
            try
            {
                var order = new CaseOrder
                {
                    CaseId = request.CaseId,
                    SupplyId = request.SupplyId,
                    Quantity = request.Quantity,
                    OrderTime = DateTime.Now
                };

                _context.CaseOrders.Add(order);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCaseOrder), new { id = order.CaseOrderId }, 
                    new { message = "個案訂單新增成功", orderId = order.CaseOrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "新增個案訂單失敗", error = ex.Message });
            }
        }

        // PUT: api/CaseOrder/5
        /// <summary>
        /// 更新個案訂單
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCaseOrder(int id, [FromBody] UpdateCaseOrderRequest request)
        {
            try
            {
                var order = await _context.CaseOrders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "找不到指定的個案訂單" });
                }

                order.CaseId = request.CaseId ?? order.CaseId;
                order.SupplyId = request.SupplyId ?? order.SupplyId;
                order.Quantity = request.Quantity ?? order.Quantity;
                // CaseOrder模型沒有Status欄位，忽略Status更新

                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "個案訂單更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新個案訂單失敗", error = ex.Message });
            }
        }

        // DELETE: api/CaseOrder/5
        /// <summary>
        /// 刪除個案訂單
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCaseOrder(int id)
        {
            try
            {
                var order = await _context.CaseOrders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "找不到指定的個案訂單" });
                }

                _context.CaseOrders.Remove(order);
                await _context.SaveChangesAsync();

                return Ok(new { message = "個案訂單刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "刪除個案訂單失敗", error = ex.Message });
            }
        }
    }

    // DTO Classes
    public class CreateCaseOrderRequest
    {
        public int CaseId { get; set; }
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCaseOrderRequest
    {
        public int? CaseId { get; set; }
        public int? SupplyId { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
    }
}