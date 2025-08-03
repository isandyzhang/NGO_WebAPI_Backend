using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models.Domain.SupplyManagement;

public partial class EmergencySupplyMatch
{
    public int EmergencyMatchId { get; set; }

    public int? EmergencyNeedId { get; set; }

    public int? MatchedQuantity { get; set; }

    public DateTime? MatchDate { get; set; }

    public int? MatchedByWorkerId { get; set; }

    public string? Note { get; set; }

    public virtual EmergencySupplyNeed? EmergencyNeed { get; set; }

    public virtual Worker? MatchedByWorker { get; set; }
}
