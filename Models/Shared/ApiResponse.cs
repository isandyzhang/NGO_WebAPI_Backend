namespace NGO_WebAPI_Backend.Models.Shared
{
    /// <summary>
    /// 統一的 API 回應格式
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 回應訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 資料內容
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 錯誤詳細資訊
        /// </summary>
        public object? Error { get; set; }

        /// <summary>
        /// 時間戳記
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立成功回應
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 建立錯誤回應
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, object? error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }

    /// <summary>
    /// 分頁回應格式
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public class PagedApiResponse<T> : ApiResponse<List<T>>
    {
        /// <summary>
        /// 分頁資訊
        /// </summary>
        public PageInfo PageInfo { get; set; } = new PageInfo();

        /// <summary>
        /// 建立分頁成功回應
        /// </summary>
        public static PagedApiResponse<T> SuccessResponse(List<T> data, int page, int pageSize, int totalCount, string message = "查詢成功")
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            return new PagedApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                PageInfo = new PageInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                }
            };
        }
    }

    /// <summary>
    /// 分頁資訊
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每頁數量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
}