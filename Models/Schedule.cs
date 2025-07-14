using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int? WorkerId { get; set; }

    public int? CaseId { get; set; }

    public string? Description { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Priority { get; set; }

    public string? Status { get; set; }

    public string EventType { get; set; } = null!;

    public string EventName { get; set; } = null!;

    public virtual Case? Case { get; set; }

    public virtual Worker? Worker { get; set; }
}
