using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class Worker
{
    public int WorkerId { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Name { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

    public virtual ICollection<EmergencySupplyMatch> EmergencySupplyMatches { get; set; } = new List<EmergencySupplyMatch>();

    public virtual ICollection<EmergencySupplyNeed> EmergencySupplyNeeds { get; set; } = new List<EmergencySupplyNeed>();

    public virtual ICollection<RegularDistributionBatch> RegularDistributionBatchApprovedByWorkers { get; set; } = new List<RegularDistributionBatch>();

    public virtual ICollection<RegularDistributionBatch> RegularDistributionBatchCreatedByWorkers { get; set; } = new List<RegularDistributionBatch>();

    public virtual ICollection<RegularSupplyMatch> RegularSupplyMatches { get; set; } = new List<RegularSupplyMatch>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
