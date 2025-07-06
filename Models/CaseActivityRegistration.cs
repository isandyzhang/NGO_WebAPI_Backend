using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_WebAPI_Backend.Models
{
    public class CaseActivityRegistration
    {
        [Key]
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int ActivityId { get; set; }
        public string Status { get; set; } = "Pending";

        // 導航屬性
        [ForeignKey("CaseId")]
        public virtual Case? Case { get; set; }
        
        [ForeignKey("ActivityId")]
        public virtual Activity? Activity { get; set; }
    }
} 