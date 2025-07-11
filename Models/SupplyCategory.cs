using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class SupplyCategory
{
    public int SupplyCategoryId { get; set; }

    public string SupplyCategoryName { get; set; } = null!;

    public virtual ICollection<Supply> Supplies { get; set; } = new List<Supply>();
}
