using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class Activity
{
    public int ActivityId { get; set; }

    public string? ActivityName { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? Location { get; set; }

     public int? MaxParticipants { get; set; }

    public int? CurrentParticipants { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateOnly? SignupDeadline { get; set; }

    public int? WorkerId { get; set; }

    public string? TargetAudience { get; set; }

    public string? Status { get; set; }

    public string? Category { get; set; }

    public virtual ICollection<CaseActivityRegistration> CaseActivityRegistrations { get; set; } = new List<CaseActivityRegistration>();

    public virtual ICollection<UserActivityRegistration> UserActivityRegistrations { get; set; } = new List<UserActivityRegistration>();

    public virtual Worker? Worker { get; set; }
}
