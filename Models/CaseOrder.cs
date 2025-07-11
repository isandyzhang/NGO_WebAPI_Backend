using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class CaseOrder
{
    public int CaseOrderId { get; set; }

    public int? CaseId { get; set; }

    public int? SupplyId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? OrderTime { get; set; }

    public virtual Case? Case { get; set; }

    public virtual Supply? Supply { get; set; }
}
