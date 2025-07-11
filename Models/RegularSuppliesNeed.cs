using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class RegularSuppliesNeed
{
    public int RegularNeedId { get; set; }

    public int? CaseId { get; set; }

    public int? SupplyId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? ApplyDate { get; set; }

    public string? Status { get; set; }

    public DateTime? PickupDate { get; set; }

    public virtual Case? Case { get; set; }

    public virtual ICollection<RegularSupplyMatch> RegularSupplyMatches { get; set; } = new List<RegularSupplyMatch>();

    public virtual Supply? Supply { get; set; }
}
