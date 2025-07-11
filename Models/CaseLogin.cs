using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class CaseLogin
{
    public int CaseId { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual Case Case { get; set; } = null!;
}
