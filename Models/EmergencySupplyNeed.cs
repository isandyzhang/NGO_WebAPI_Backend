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

    public virtual Case? Case { get; set; }

    public virtual ICollection<EmergencySupplyMatch> EmergencySupplyMatches { get; set; } = new List<EmergencySupplyMatch>();

    public virtual Supply? Supply { get; set; }

    public virtual Worker? Worker { get; set; }
}
