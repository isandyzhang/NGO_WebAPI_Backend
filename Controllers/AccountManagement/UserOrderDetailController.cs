using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;

namespace NGO_WebAPI_Backend.Controllers.AccountManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserOrderDetailController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public UserOrderDetailController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/UserOrderDetail/{orderId}
        /// <summary>
        /// 取得指定訂單的所有詳情
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserOrderDetails(int orderId)
        {
            try
            {
                var orderDetails = await _context.UserOrderDetails
                    .Include(d => d.Supply)
                    .Where(d => d.UserOrderId == orderId)
                    .Select(d => new
                    {
                        orderDetailId = d.DetailId,
                        userOrderId = d.UserOrderId,
                        supplyId = d.SupplyId,
                        supplyName = d.Supply != null ? d.Supply.SupplyName : "未知物資",
                        quantity = d.Quantity ?? 0,
                        unitPrice = d.UnitPrice ?? 0,
                        totalPrice = (d.Quantity ?? 0) * (d.UnitPrice ?? 0)
                    })
                    .ToListAsync();

                return Ok(orderDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得用戶訂單詳情失敗", error = ex.Message });
            }
        }

        // GET: api/UserOrderDetail/detail/{id}
        /// <summary>
        /// 取得單一訂單詳情
        /// </summary>
        [HttpGet("detail/{id}")]
        public async Task<ActionResult<object>> GetUserOrderDetail(int id)
        {
            try
            {
                var orderDetail = await _context.UserOrderDetails
                    .Include(d => d.Supply)
                    .Where(d => d.DetailId == id)
                    .Select(d => new
                    {
                        orderDetailId = d.DetailId,
                        userOrderId = d.UserOrderId,
                        supplyId = d.SupplyId,
                        supplyName = d.Supply != null ? d.Supply.SupplyName : "未知物資",
                        quantity = d.Quantity ?? 0,
                        unitPrice = d.UnitPrice ?? 0,
                        totalPrice = (d.Quantity ?? 0) * (d.UnitPrice ?? 0)
                    })
                    .FirstOrDefaultAsync();

                if (orderDetail == null)
                {
                    return NotFound(new { message = "找不到指定的訂單詳情" });
                }

                return Ok(orderDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得訂單詳情失敗", error = ex.Message });
            }
        }

        // POST: api/UserOrderDetail
        /// <summary>
        /// 新增訂單詳情
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> PostUserOrderDetail([FromBody] CreateUserOrderDetailRequest request)
        {
            try
            {
                var orderDetail = new UserOrderDetail
                {
                    UserOrderId = request.UserOrderId,
                    SupplyId = request.SupplyId,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice
                };

                _context.UserOrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserOrderDetail), new { id = orderDetail.DetailId }, 
                    new { message = "訂單詳情新增成功", detailId = orderDetail.DetailId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "新增訂單詳情失敗", error = ex.Message });
            }
        }

        // PUT: api/UserOrderDetail/5
        /// <summary>
        /// 更新訂單詳情
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserOrderDetail(int id, [FromBody] UpdateUserOrderDetailRequest request)
        {
            try
            {
                var orderDetail = await _context.UserOrderDetails.FindAsync(id);
                if (orderDetail == null)
                {
                    return NotFound(new { message = "找不到指定的訂單詳情" });
                }

                orderDetail.SupplyId = request.SupplyId ?? orderDetail.SupplyId;
                orderDetail.Quantity = request.Quantity ?? orderDetail.Quantity;
                orderDetail.UnitPrice = request.UnitPrice ?? orderDetail.UnitPrice;

                _context.Entry(orderDetail).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "訂單詳情更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新訂單詳情失敗", error = ex.Message });
            }
        }

        // DELETE: api/UserOrderDetail/5
        /// <summary>
        /// 刪除訂單詳情
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserOrderDetail(int id)
        {
            try
            {
                var orderDetail = await _context.UserOrderDetails.FindAsync(id);
                if (orderDetail == null)
                {
                    return NotFound(new { message = "找不到指定的訂單詳情" });
                }

                _context.UserOrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();

                return Ok(new { message = "訂單詳情刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "刪除訂單詳情失敗", error = ex.Message });
            }
        }
    }

    // DTO Classes
    public class CreateUserOrderDetailRequest
    {
        public int UserOrderId { get; set; }
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UpdateUserOrderDetailRequest
    {
        public int? SupplyId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}