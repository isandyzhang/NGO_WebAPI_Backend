using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class UserOrder
{
    public int UserOrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? PaymentStatus { get; set; }

    public decimal? TotalPrice { get; set; }

    public string OrderNumber { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual ICollection<UserOrderDetail> UserOrderDetails { get; set; } = new List<UserOrderDetail>();
}
