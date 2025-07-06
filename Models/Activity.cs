using System.ComponentModel.DataAnnotations;

namespace NGO_WebAPI_Backend.Models
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ActivityName { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
        
        public int MaxParticipants { get; set; }
        
        public int CurrentParticipants { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public DateTime? SignupDeadline { get; set; }
        
        public int WorkerId { get; set; }
        
        [StringLength(100)]
        public string? TargetAudience { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active";
        
        // 導航屬性
        public virtual Worker? Worker { get; set; }
        public virtual ICollection<CaseActivityRegistration>? CaseActivityRegistrations { get; set; }
        public virtual ICollection<UserActivityRegistration>? UserActivityRegistrations { get; set; }
    }
} 