using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class RegularDistributionBatch
{
    public int DistributionBatchId { get; set; }

    public DateTime DistributionDate { get; set; }

    public int CaseCount { get; set; }

    public int TotalSupplyItems { get; set; }

    public int CreatedByWorkerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int? ApprovedByWorkerId { get; set; }

    public string? Notes { get; set; }

    public virtual Worker? ApprovedByWorker { get; set; }

    public virtual Worker CreatedByWorker { get; set; } = null!;
}
