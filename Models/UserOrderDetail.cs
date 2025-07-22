using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class UserOrderDetail
{
    public int DetailId { get; set; }

    public int? UserOrderId { get; set; }

    public int? SupplyId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public string? OrderSource { get; set; }

    public int? EmergencyNeedId { get; set; }

    public virtual Supply? Supply { get; set; }

    public virtual UserOrder? UserOrder { get; set; }
}
