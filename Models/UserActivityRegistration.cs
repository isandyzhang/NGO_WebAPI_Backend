using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_WebAPI_Backend.Models
{
    public class UserActivityRegistration
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public string Status { get; set; } = "Pending";
        public int? NumberOfCompanions { get; set; }

        // 導航屬性
        [ForeignKey("ActivityId")]
        public virtual Activity? Activity { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
} 