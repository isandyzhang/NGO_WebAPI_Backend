using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class RegularSupplyMatch
{
    public int RegularMatchId { get; set; }

    public int? RegularNeedId { get; set; }

    public int? MatchedQuantity { get; set; }

    public DateTime? MatchDate { get; set; }

    public int? MatchedByWorkerId { get; set; }

    public string? Note { get; set; }

    public virtual Worker? MatchedByWorker { get; set; }

    public virtual RegularSuppliesNeed? RegularNeed { get; set; }
}
