using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Repositories
{
    /// <summary>
    /// 個案資料存取介面
    /// </summary>
    public interface ICaseRepository
    {
        /// <summary>
        /// 取得所有個案（支援分頁和過濾）
        /// </summary>
        Task<(List<Case> Cases, int TotalCount)> GetAllCasesAsync(int page, int pageSize, int? workerId = null);

        /// <summary>
        /// 根據 ID 取得個案
        /// </summary>
        Task<Case?> GetCaseByIdAsync(int id);

        /// <summary>
        /// 根據身分證字號取得個案
        /// </summary>
        Task<Case?> GetCaseByIdentityNumberAsync(string identityNumber);

        /// <summary>
        /// 搜尋個案
        /// </summary>
        Task<(List<Case> Cases, int TotalCount)> SearchCasesAsync(string? query, int page, int pageSize, int? workerId = null);

        /// <summary>
        /// 建立個案
        /// </summary>
        Task<Case> CreateCaseAsync(Case caseEntity);

        /// <summary>
        /// 更新個案
        /// </summary>
        Task<Case> UpdateCaseAsync(Case caseEntity);

        /// <summary>
        /// 刪除個案
        /// </summary>
        Task<bool> DeleteCaseAsync(int id);

        /// <summary>
        /// 檢查個案是否有相關資料
        /// </summary>
        Task<List<string>> GetCaseRelatedDataAsync(int id);

        /// <summary>
        /// 檢查身分證字號是否已存在
        /// </summary>
        Task<bool> IsIdentityNumberExistsAsync(string identityNumber, int? excludeCaseId = null);
    }
}