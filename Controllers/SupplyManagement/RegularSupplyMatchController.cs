using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;

namespace NGO_WebAPI_Backend.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegularSupplyMatchController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public RegularSupplyMatchController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/RegularSupplyMatch
        /// <summary>
        /// 取得所有常駐物資配對
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRegularSupplyMatches()
        {
            try
            {
                var matches = await _context.RegularSupplyMatches
                    .Include(m => m.RegularNeed)
                        .ThenInclude(n => n!.Case)
                    .Include(m => m.MatchedByWorker)
                    .Select(m => new
                    {
                        regularMatchId = m.RegularMatchId,
                        regularNeedId = m.RegularNeedId,
                        supplyId = 0, // RegularSupplyMatch模型沒有SupplyId欄位
                        matchedByWorkerId = m.MatchedByWorkerId,
                        matchedByWorkerName = m.MatchedByWorker != null ? m.MatchedByWorker.Name : "未知",
                        matchDate = m.MatchDate != null ? m.MatchDate.Value.ToString("yyyy-MM-dd") : "",
                        note = m.Note,
                        status = "matched" // RegularSupplyMatch模型沒有Status欄位，使用固定值
                    })
                    .ToListAsync();

                return Ok(matches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得常駐物資配對失敗", error = ex.Message });
            }
        }

        // POST: api/RegularSupplyMatch
        /// <summary>
        /// 新增常駐物資配對
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> PostRegularSupplyMatch([FromBody] CreateRegularSupplyMatchRequest request)
        {
            try
            {
                var match = new RegularSupplyMatch
                {
                    RegularNeedId = request.RegularNeedId,
                    MatchedByWorkerId = request.MatchedByWorkerId,
                    MatchDate = DateTime.Now,
                    Note = request.Note
                };

                _context.RegularSupplyMatches.Add(match);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRegularSupplyMatches), 
                    new { message = "常駐物資配對新增成功", matchId = match.RegularMatchId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "新增常駐物資配對失敗", error = ex.Message });
            }
        }

        // PUT: api/RegularSupplyMatch/5
        /// <summary>
        /// 更新常駐物資配對狀態
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegularSupplyMatch(int id, [FromBody] UpdateRegularSupplyMatchRequest request)
        {
            try
            {
                var match = await _context.RegularSupplyMatches.FindAsync(id);
                if (match == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資配對" });
                }

                // RegularSupplyMatch模型沒有Status欄位，只更新Note
                match.Note = request.Note ?? match.Note;

                _context.Entry(match).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "常駐物資配對更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新常駐物資配對失敗", error = ex.Message });
            }
        }

        // DELETE: api/RegularSupplyMatch/5
        /// <summary>
        /// 刪除常駐物資配對
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegularSupplyMatch(int id)
        {
            try
            {
                var match = await _context.RegularSupplyMatches.FindAsync(id);
                if (match == null)
                {
                    return NotFound(new { message = "找不到指定的常駐物資配對" });
                }

                _context.RegularSupplyMatches.Remove(match);
                await _context.SaveChangesAsync();

                return Ok(new { message = "常駐物資配對刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "刪除常駐物資配對失敗", error = ex.Message });
            }
        }
    }

    // DTO Classes
    public class CreateRegularSupplyMatchRequest
    {
        public int RegularNeedId { get; set; }
        public int MatchedByWorkerId { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateRegularSupplyMatchRequest
    {
        public string? Status { get; set; }
        public string? Note { get; set; }
    }
}