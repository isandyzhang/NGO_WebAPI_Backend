using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencySupplyMatchController : ControllerBase
    {
        private readonly NgoplatformDbContext _context;

        public EmergencySupplyMatchController(NgoplatformDbContext context)
        {
            _context = context;
        }

        // GET: api/EmergencySupplyMatch
        /// <summary>
        /// 取得所有緊急物資配對
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEmergencySupplyMatches()
        {
            try
            {
                var matches = await _context.EmergencySupplyMatches
                    .Include(m => m.EmergencyNeed)
                        .ThenInclude(n => n!.Case)
                    .Include(m => m.MatchedByWorker)
                    .Select(m => new
                    {
                        emergencyMatchId = m.EmergencyMatchId,
                        emergencyNeedId = m.EmergencyNeedId,
                        supplyId = 0, // EmergencySupplyMatch模型沒有SupplyId欄位
                        matchedByWorkerId = m.MatchedByWorkerId,
                        matchedByWorkerName = m.MatchedByWorker != null ? m.MatchedByWorker.Name : "未知",
                        matchDate = m.MatchDate != null ? m.MatchDate.Value.ToString("yyyy-MM-dd") : "",
                        note = m.Note,
                        status = "matched" // EmergencySupplyMatch模型沒有Status欄位，使用固定值
                    })
                    .ToListAsync();

                return Ok(matches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得緊急物資配對失敗", error = ex.Message });
            }
        }

        // POST: api/EmergencySupplyMatch
        /// <summary>
        /// 新增緊急物資配對
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> PostEmergencySupplyMatch([FromBody] CreateEmergencySupplyMatchRequest request)
        {
            try
            {
                var match = new EmergencySupplyMatch
                {
                    EmergencyNeedId = request.EmergencyNeedId,
                    MatchedByWorkerId = request.MatchedByWorkerId,
                    MatchDate = DateTime.Now,
                    Note = request.Note
                };

                _context.EmergencySupplyMatches.Add(match);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEmergencySupplyMatches), 
                    new { message = "緊急物資配對新增成功", matchId = match.EmergencyMatchId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "新增緊急物資配對失敗", error = ex.Message });
            }
        }

        // PUT: api/EmergencySupplyMatch/5
        /// <summary>
        /// 更新緊急物資配對狀態
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmergencySupplyMatch(int id, [FromBody] UpdateEmergencySupplyMatchRequest request)
        {
            try
            {
                var match = await _context.EmergencySupplyMatches.FindAsync(id);
                if (match == null)
                {
                    return NotFound(new { message = "找不到指定的緊急物資配對" });
                }

                // EmergencySupplyMatch模型沒有Status欄位，只更新Note
                match.Note = request.Note ?? match.Note;

                _context.Entry(match).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "緊急物資配對更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新緊急物資配對失敗", error = ex.Message });
            }
        }

        // DELETE: api/EmergencySupplyMatch/5
        /// <summary>
        /// 刪除緊急物資配對
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmergencySupplyMatch(int id)
        {
            try
            {
                var match = await _context.EmergencySupplyMatches.FindAsync(id);
                if (match == null)
                {
                    return NotFound(new { message = "找不到指定的緊急物資配對" });
                }

                _context.EmergencySupplyMatches.Remove(match);
                await _context.SaveChangesAsync();

                return Ok(new { message = "緊急物資配對刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "刪除緊急物資配對失敗", error = ex.Message });
            }
        }
    }

    // DTO Classes
    public class CreateEmergencySupplyMatchRequest
    {
        public int EmergencyNeedId { get; set; }
        public int MatchedByWorkerId { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateEmergencySupplyMatchRequest
    {
        public string? Status { get; set; }
        public string? Note { get; set; }
    }
}