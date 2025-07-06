using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_WebAPI_Backend.Models
{
    /// <summary>
    /// 工作人員模型 - 對應資料庫的Workers表
    /// </summary>
    [Table("Workers")]
    public class Worker
    {
        /// <summary>
        /// 工作人員ID - 主鍵
        /// </summary>
        [Key]
        [Column("WorkerId")]
        public int WorkerId { get; set; }

        /// <summary>
        /// 電子郵件 - 用於登入的帳號
        /// </summary>
        [Required]
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密碼 - 用於登入驗證
        /// </summary>
        [Required]
        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 姓名 - 工作人員的姓名
        /// </summary>
        [Required]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;
    }
}

/// <summary>
/// 登入請求模型 - 用於接收前端的登入資料
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 電子郵件
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 登入回應模型 - 用於回傳登入結果
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 是否登入成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 工作人員資訊（成功時回傳）
    /// </summary>
    public WorkerInfo? Worker { get; set; }
}

/// <summary>
/// 工作人員資訊模型 - 用於回傳給前端（不包含密碼）
/// </summary>
public class WorkerInfo
{
    /// <summary>
    /// 工作人員ID
    /// </summary>
    public int WorkerId { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;
} 