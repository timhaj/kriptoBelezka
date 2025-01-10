using System;
using System.Collections.Generic;

namespace web.Models;

public class Nastavitve
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public bool IsDarkMode { get; set; }
    public string CurrentCurrencySelected { get; set; }
    public string? ApiKey { get; set; }

    // Navigation property
    public User? User { get; set; }
    public ApplicationUser? OwnerId { get; set; }
}
