using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_WebAPI_Backend.Models
{
    /// <summary>
    /// 個案模型 - 對應資料庫的Cases表
    /// </summary>
    [Table("Cases")]
    public class Case
    {
        /// <summary>
        /// 個案ID - 主鍵
        /// </summary>
        [Key]
        [Column("CaseId")]
        public int CaseId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Required]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 電話
        /// </summary>
        [Required]
        [Column("Phone")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Required]
        [Column("IdentityNumber")]
        public string IdentityNumber { get; set; } = string.Empty;

        /// <summary>
        /// 生日
        /// </summary>
        [Column("Birthday")]
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 地址（完整地址）
        /// </summary>
        [Column("Address")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 負責工作人員ID
        /// </summary>
        [Column("WorkerId")]
        public int WorkerId { get; set; }

        /// <summary>
        /// 描述/困難類型
        /// </summary>
        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 狀態
        /// </summary>
        [Column("Status")]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// 電子郵件
        /// </summary>
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 性別
        /// </summary>
        [Column("Gender")]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 個人照片
        /// </summary>
        [Column("ProfileImage")]
        public string? ProfileImage { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [Column("City")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 地區
        /// </summary>
        [Column("District")]
        public string District { get; set; } = string.Empty;

        /// <summary>
        /// 詳細地址
        /// </summary>
        [Column("DetailAddress")]
        public string DetailAddress { get; set; } = string.Empty;

        /// <summary>
        /// 導航屬性 - 負責工作人員
        /// </summary>
        [ForeignKey("WorkerId")]
        public virtual Worker? Worker { get; set; }

        /// <summary>
        /// 導航屬性 - 個案活動報名
        /// </summary>
        public virtual ICollection<CaseActivityRegistration>? CaseActivityRegistrations { get; set; }
    }

    /// <summary>
    /// 建立個案請求模型
    /// </summary>
    public class CreateCaseRequest
    {
        /// <summary>
        /// 姓名
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 電話
        /// </summary>
        [Required]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Required]
        public string IdentityNumber { get; set; } = string.Empty;

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 描述/困難類型
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 電子郵件
        /// </summary>
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 性別
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 個人照片
        /// </summary>
        public string? ProfileImage { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 地區
        /// </summary>
        public string District { get; set; } = string.Empty;

        /// <summary>
        /// 詳細地址
        /// </summary>
        public string DetailAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新個案請求模型
    /// </summary>
    public class UpdateCaseRequest
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        public string? IdentityNumber { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 描述/困難類型
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// 個人照片
        /// </summary>
        public string? ProfileImage { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 地區
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// 詳細地址
        /// </summary>
        public string? DetailAddress { get; set; }
    }

    /// <summary>
    /// 個案回應模型
    /// </summary>
    public class CaseResponse
    {
        /// <summary>
        /// 個案ID
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 電話
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 身分證字號
        /// </summary>
        public string IdentityNumber { get; set; } = string.Empty;

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 負責工作人員ID
        /// </summary>
        public int WorkerId { get; set; }

        /// <summary>
        /// 描述/困難類型
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 電子郵件
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 性別
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 個人照片
        /// </summary>
        public string? ProfileImage { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 地區
        /// </summary>
        public string District { get; set; } = string.Empty;

        /// <summary>
        /// 詳細地址
        /// </summary>
        public string DetailAddress { get; set; } = string.Empty;

        /// <summary>
        /// 負責工作人員姓名
        /// </summary>
        public string? WorkerName { get; set; }
    }
} 