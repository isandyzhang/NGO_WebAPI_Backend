using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class VwActivitiesFrontend
{
    public int ActivityId { get; set; }

    public string? ActivityName { get; set; }

    public string? Location { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? CurrentParticipants { get; set; }

    public int? MaxParticipants { get; set; }

    public string? DisplayStatus { get; set; }
}
