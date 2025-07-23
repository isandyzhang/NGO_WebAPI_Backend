using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class Case
{
    public int CaseId { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? IdentityNumber { get; set; }

    public DateOnly? Birthday { get; set; }

    public int? WorkerId { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public string? ProfileImage { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public string? DetailAddress { get; set; }

    // public string? SpeechToText { get; set; }

    public string? SpeechToTextAudioUrl { get; set; }

    public virtual ICollection<CaseActivityRegistration> CaseActivityRegistrations { get; set; } = new List<CaseActivityRegistration>();

    public virtual CaseLogin? CaseLogin { get; set; }

    public virtual ICollection<CaseOrder> CaseOrders { get; set; } = new List<CaseOrder>();

    public virtual ICollection<EmergencySupplyNeed> EmergencySupplyNeeds { get; set; } = new List<EmergencySupplyNeed>();

    public virtual ICollection<RegularSuppliesNeed> RegularSuppliesNeeds { get; set; } = new List<RegularSuppliesNeed>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual Worker? Worker { get; set; }
}
