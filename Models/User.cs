using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? IdentityNumber { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<UserActivityRegistration> UserActivityRegistrations { get; set; } = new List<UserActivityRegistration>();

    public virtual ICollection<UserOrder> UserOrders { get; set; } = new List<UserOrder>();
}
