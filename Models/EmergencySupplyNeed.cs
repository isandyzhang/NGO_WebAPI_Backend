using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class EmergencySupplyNeed
{
    public int EmergencyNeedId { get; set; }

    public int? CaseId { get; set; }

    public int? WorkerId { get; set; }

    public int? Quantity { get; set; }

    public int? CollectedQuantity { get; set; }

    public string? SupplyName { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Case? Case { get; set; }

    public virtual ICollection<EmergencySupplyMatch> EmergencySupplyMatches { get; set; } = new List<EmergencySupplyMatch>();

    public virtual Worker? Worker { get; set; }
}
