using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class VwActivitiesStatus
{
    public int ActivityId { get; set; }

    public string? ActivityName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Location { get; set; }

    public int? CurrentParticipants { get; set; }

    public int? MaxParticipants { get; set; }

    public string? 原始狀態 { get; set; }

    public string 系統建議狀態 { get; set; } = null!;

    public string 中文狀態 { get; set; } = null!;

    public string? TimeBasedStatus { get; set; }

    public int? DaysRemaining { get; set; }

    public string 狀態檢查 { get; set; } = null!;

    public DateOnly? SignupDeadline { get; set; }

    public string? Category { get; set; }

    public string? TargetAudience { get; set; }
}
