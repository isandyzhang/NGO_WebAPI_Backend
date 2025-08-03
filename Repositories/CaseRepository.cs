using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models.Infrastructure;
using NGO_WebAPI_Backend.Models.Domain.CaseManagement;

namespace NGO_WebAPI_Backend.Repositories
{
    /// <summary>
    /// 個案資料存取實作
    /// </summary>
    public class CaseRepository : ICaseRepository
    {
        private readonly NgoplatformDbContext _context;
        private readonly ILogger<CaseRepository> _logger;

        public CaseRepository(NgoplatformDbContext context, ILogger<CaseRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有個案（支援分頁和過濾）
        /// </summary>
        public async Task<(List<Case> Cases, int TotalCount)> GetAllCasesAsync(int page, int pageSize, int? workerId = null)
        {
            var queryable = _context.Cases
                .Include(c => c.Worker)
                .AsQueryable();

            // WorkerId 過濾
            if (workerId.HasValue)
            {
                queryable = queryable.Where(c => c.WorkerId == workerId.Value);
            }

            queryable = queryable.OrderByDescending(c => c.CreatedAt);

            var totalCount = await queryable.CountAsync();
            var cases = await queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cases, totalCount);
        }

        /// <summary>
        /// 根據 ID 取得個案
        /// </summary>
        public async Task<Case?> GetCaseByIdAsync(int id)
        {
            return await _context.Cases
                .Include(c => c.Worker)
                .FirstOrDefaultAsync(c => c.CaseId == id);
        }

        /// <summary>
        /// 根據身分證字號取得個案
        /// </summary>
        public async Task<Case?> GetCaseByIdentityNumberAsync(string identityNumber)
        {
            return await _context.Cases
                .FirstOrDefaultAsync(c => c.IdentityNumber == identityNumber);
        }

        /// <summary>
        /// 搜尋個案
        /// </summary>
        public async Task<(List<Case> Cases, int TotalCount)> SearchCasesAsync(string? query, int page, int pageSize, int? workerId = null)
        {
            var queryable = _context.Cases
                .Include(c => c.Worker)
                .AsQueryable();

            // WorkerId 過濾
            if (workerId.HasValue)
            {
                queryable = queryable.Where(c => c.WorkerId == workerId.Value);
            }

            // 關鍵字搜尋
            if (!string.IsNullOrWhiteSpace(query))
            {
                queryable = queryable.Where(c =>
                    (c.Name != null && c.Name.Contains(query)) ||
                    (c.Phone != null && c.Phone.Contains(query)) ||
                    (c.IdentityNumber != null && c.IdentityNumber.Contains(query)) ||
                    (c.Email != null && c.Email.Contains(query)) ||
                    (c.Description != null && c.Description.Contains(query)) ||
                    (c.City != null && c.City.Contains(query)) ||
                    (c.District != null && c.District.Contains(query))
                );
            }

            queryable = queryable.OrderByDescending(c => c.CreatedAt);

            var totalCount = await queryable.CountAsync();
            var cases = await queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cases, totalCount);
        }

        /// <summary>
        /// 建立個案
        /// </summary>
        public async Task<Case> CreateCaseAsync(Case caseEntity)
        {
            _context.Cases.Add(caseEntity);
            await _context.SaveChangesAsync();

            // 重新載入以取得完整資料
            return await GetCaseByIdAsync(caseEntity.CaseId) ?? caseEntity;
        }

        /// <summary>
        /// 更新個案
        /// </summary>
        public async Task<Case> UpdateCaseAsync(Case caseEntity)
        {
            _context.Cases.Update(caseEntity);
            await _context.SaveChangesAsync();

            // 重新載入以取得完整資料
            return await GetCaseByIdAsync(caseEntity.CaseId) ?? caseEntity;
        }

        /// <summary>
        /// 刪除個案
        /// </summary>
        public async Task<bool> DeleteCaseAsync(int id)
        {
            var caseEntity = await _context.Cases.FindAsync(id);
            if (caseEntity == null)
            {
                return false;
            }

            _context.Cases.Remove(caseEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 檢查個案是否有相關資料
        /// </summary>
        public async Task<List<string>> GetCaseRelatedDataAsync(int id)
        {
            var relatedData = new List<string>();

            // 檢查緊急物資需求
            var emergencySupplyNeeds = await _context.EmergencySupplyNeeds
                .Where(e => e.CaseId == id)
                .CountAsync();
            if (emergencySupplyNeeds > 0)
            {
                relatedData.Add($"緊急物資需求 ({emergencySupplyNeeds} 筆)");
            }

            // 檢查常駐物資需求
            var regularSuppliesNeeds = await _context.RegularSuppliesNeeds
                .Where(r => r.CaseId == id)
                .CountAsync();
            if (regularSuppliesNeeds > 0)
            {
                relatedData.Add($"常駐物資需求 ({regularSuppliesNeeds} 筆)");
            }

            // 檢查個案訂單
            var caseOrders = await _context.CaseOrders
                .Where(c => c.CaseId == id)
                .CountAsync();
            if (caseOrders > 0)
            {
                relatedData.Add($"個案訂單 ({caseOrders} 筆)");
            }

            // 檢查活動報名
            var caseActivityRegistrations = await _context.CaseActivityRegistrations
                .Where(c => c.CaseId == id)
                .CountAsync();
            if (caseActivityRegistrations > 0)
            {
                relatedData.Add($"活動報名 ({caseActivityRegistrations} 筆)");
            }

            // 檢查行程安排
            var schedules = await _context.Schedules
                .Where(s => s.CaseId == id)
                .CountAsync();
            if (schedules > 0)
            {
                relatedData.Add($"行程安排 ({schedules} 筆)");
            }

            // 檢查個案登入資料
            var caseLogin = await _context.CaseLogins
                .Where(c => c.CaseId == id)
                .CountAsync();
            if (caseLogin > 0)
            {
                relatedData.Add("個案登入資料 (1 筆)");
            }

            return relatedData;
        }

        /// <summary>
        /// 檢查身分證字號是否已存在
        /// </summary>
        public async Task<bool> IsIdentityNumberExistsAsync(string identityNumber, int? excludeCaseId = null)
        {
            var query = _context.Cases.Where(c => c.IdentityNumber == identityNumber);
            
            if (excludeCaseId.HasValue)
            {
                query = query.Where(c => c.CaseId != excludeCaseId.Value);
            }

            return await query.AnyAsync();
        }
    }
}