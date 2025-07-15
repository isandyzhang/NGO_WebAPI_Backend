using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class CaseActivityRegistration
{
    public int RegistrationId { get; set; }

    public int? CaseId { get; set; }

    public int? ActivityId { get; set; }

    public string? Status { get; set; }

    public DateTime? RegisterTime { get; set; }

    public virtual Activity? Activity { get; set; }

    public virtual Case? Case { get; set; }
}
