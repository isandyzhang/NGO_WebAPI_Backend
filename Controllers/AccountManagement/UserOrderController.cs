using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;

namespace NGO_WebAPI_Backend.Controllers.AccountManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserOrderController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public UserOrderController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/UserOrder
        /// <summary>
        /// 取得所有用戶訂單
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUserOrders()
        {
            try
            {
                // Note: 由於沒有 Users 表，這裡使用 Worker 作為替代
                var orders = await _context.UserOrders
                    .Select(o => new
                    {
                        userOrderId = o.UserOrderId,
                        userId = o.UserId ?? 0,
                        userName = "用戶", // 暫時固定，後續可關聯 Worker 或其他用戶表
                        orderDate = o.OrderDate != null ? o.OrderDate.Value.ToString("yyyy-MM-dd") : "",
                        status = o.PaymentStatus ?? "pending",
                        totalAmount = o.TotalPrice ?? 0
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得用戶訂單失敗", error = ex.Message });
            }
        }

        // GET: api/UserOrder/5
        /// <summary>
        /// 取得單一用戶訂單
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUserOrder(int id)
        {
            try
            {
                var order = await _context.UserOrders
                    .Where(o => o.UserOrderId == id)
                    .Select(o => new
                    {
                        userOrderId = o.UserOrderId,
                        userId = o.UserId ?? 0,
                        userName = "用戶",
                        orderDate = o.OrderDate != null ? o.OrderDate.Value.ToString("yyyy-MM-dd") : "",
                        status = o.PaymentStatus ?? "pending",
                        totalAmount = o.TotalPrice ?? 0
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new { message = "找不到指定的用戶訂單" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得用戶訂單失敗", error = ex.Message });
            }
        }

        // POST: api/UserOrder
        /// <summary>
        /// 新增用戶訂單
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> PostUserOrder([FromBody] CreateUserOrderRequest request)
        {
            try
            {
                var order = new UserOrder
                {
                    UserId = request.UserId,
                    OrderDate = DateTime.Now,
                    PaymentStatus = "pending",
                    TotalPrice = request.TotalAmount,
                    OrderNumber = Guid.NewGuid().ToString()
                };

                _context.UserOrders.Add(order);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserOrder), new { id = order.UserOrderId }, 
                    new { message = "用戶訂單新增成功", orderId = order.UserOrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "新增用戶訂單失敗", error = ex.Message });
            }
        }

        // PUT: api/UserOrder/5
        /// <summary>
        /// 更新用戶訂單
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserOrder(int id, [FromBody] UpdateUserOrderRequest request)
        {
            try
            {
                var order = await _context.UserOrders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "找不到指定的用戶訂單" });
                }

                order.UserId = request.UserId ?? order.UserId;
                order.PaymentStatus = request.Status ?? order.PaymentStatus;
                order.TotalPrice = request.TotalAmount ?? order.TotalPrice;

                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "用戶訂單更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新用戶訂單失敗", error = ex.Message });
            }
        }

        // DELETE: api/UserOrder/5
        /// <summary>
        /// 刪除用戶訂單
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserOrder(int id)
        {
            try
            {
                var order = await _context.UserOrders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "找不到指定的用戶訂單" });
                }

                _context.UserOrders.Remove(order);
                await _context.SaveChangesAsync();

                return Ok(new { message = "用戶訂單刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "刪除用戶訂單失敗", error = ex.Message });
            }
        }
    }

    // DTO Classes
    public class CreateUserOrderRequest
    {
        public int? UserId { get; set; }
        public decimal? TotalAmount { get; set; }
    }

    public class UpdateUserOrderRequest
    {
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}