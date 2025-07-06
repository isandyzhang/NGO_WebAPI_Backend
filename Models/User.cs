using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_WebAPI_Backend.Models
{
    /// <summary>
    /// 使用者模型 - 對應資料庫的Users表
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// 使用者ID - 主鍵
        /// </summary>
        [Key]
        [Column("UserId")]
        public int UserId { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Required]
        [Column("IdentityNumber")]
        public string IdentityNumber { get; set; } = string.Empty;

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Required]
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密碼
        /// </summary>
        [Required]
        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 電話
        /// </summary>
        [Required]
        [Column("Phone")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 姓名
        /// </summary>
        [Required]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 導航屬性 - 使用者活動報名
        /// </summary>
        public virtual ICollection<UserActivityRegistration>? UserActivityRegistrations { get; set; }
    }
} 