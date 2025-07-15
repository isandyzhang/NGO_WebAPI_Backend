using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class UserActivityRegistration
{
    public int RegistrationId { get; set; }

    public int? UserId { get; set; }

    public int? ActivityId { get; set; }

    public string? Status { get; set; }

    public int? NumberOfCompanions { get; set; }

    public DateTime? RegisterTime { get; set; }

    public virtual Activity? Activity { get; set; }

    public virtual User? User { get; set; }
}
