using System;
using System.Collections.Generic;

namespace NGO_WebAPI_Backend.Models;

public partial class PasswordResetToken
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }
}
