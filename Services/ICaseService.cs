using NGO_WebAPI_Backend.Models.Shared;
using NGO_WebAPI_Backend.DTOs;

namespace NGO_WebAPI_Backend.Services
{
    /// <summary>
    /// 個案服務介面
    /// </summary>
    public interface ICaseService
    {
        /// <summary>
        /// 取得所有個案（支援分頁和過濾）
        /// </summary>
        Task<PagedApiResponse<CaseDto>> GetAllCasesAsync(int page, int pageSize, int? workerId = null);

        /// <summary>
        /// 根據 ID 取得個案
        /// </summary>
        Task<ApiResponse<CaseDto>> GetCaseByIdAsync(int id);

        /// <summary>
        /// 搜尋個案
        /// </summary>
        Task<PagedApiResponse<CaseDto>> SearchCasesAsync(string? query, int page, int pageSize, int? workerId = null);

        /// <summary>
        /// 建立個案
        /// </summary>
        Task<ApiResponse<CaseDto>> CreateCaseAsync(CreateCaseDto createCaseDto);

        /// <summary>
        /// 更新個案
        /// </summary>
        Task<ApiResponse<CaseDto>> UpdateCaseAsync(int id, UpdateCaseDto updateCaseDto);

        /// <summary>
        /// 刪除個案
        /// </summary>
        Task<ApiResponse<bool>> DeleteCaseAsync(int id);

        /// <summary>
        /// 上傳個案圖片
        /// </summary>
        Task<ApiResponse<string>> UploadProfileImageAsync(IFormFile file);
    }
}