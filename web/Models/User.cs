using System;
using System.Collections.Generic;

namespace web.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }

    // Navigation properties
    public Nastavitve? Settings { get; set; } // One-to-One
    public Watchlist? Watchlist { get; set; } // One-to-One
    public ICollection<Portfolio>? Portfolios { get; set; } // One-to-Many
}
