using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class EmergencySupplyNeed
{
    public int EmergencyNeedId { get; set; }

    public int? CaseId { get; set; }

    public int? SupplyId { get; set; }

    public int? WorkerId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? VisitDate { get; set; }

    public string? Status { get; set; }

    public DateTime? PickupDate { get; set; }

    // 新增前端需要的字段
    public string? ItemName { get; set; }

    public string? Category { get; set; }

    public string? Unit { get; set; }

    public string? Urgency { get; set; } // 'low', 'medium', 'high'

    public string? RequestedBy { get; set; }

    public DateTime? RequestDate { get; set; }

    public decimal? EstimatedCost { get; set; }

    public string? EmergencyReason { get; set; }

    public bool? Matched { get; set; }

    public virtual Case? Case { get; set; }

    public virtual ICollection<EmergencySupplyMatch> EmergencySupplyMatches { get; set; } = new List<EmergencySupplyMatch>();

    public virtual Supply? Supply { get; set; }

    public virtual Worker? Worker { get; set; }
}
