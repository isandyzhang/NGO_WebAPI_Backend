using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models.Domain.SupplyManagement;

public partial class Supply
{
    public int SupplyId { get; set; }

    public string SupplyName { get; set; } = null!;

    public decimal? SupplyPrice { get; set; }

    public int? SupplyCategoryId { get; set; }

    public int? SupplyQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public string? SupplyDescription { get; set; }

    public string? SupplyType { get; set; }

    public virtual ICollection<CaseOrder> CaseOrders { get; set; } = new List<CaseOrder>();

    public virtual ICollection<RegularSuppliesNeed> RegularSuppliesNeeds { get; set; } = new List<RegularSuppliesNeed>();

    public virtual SupplyCategory? SupplyCategory { get; set; }

    public virtual ICollection<UserOrderDetail> UserOrderDetails { get; set; } = new List<UserOrderDetail>();
}
