using System;
using System.Collections.Generic;

namespace web.Models;

public class Watchlist
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<WatchlistAsset>? WatchlistAssets { get; set; } // Many-to-Many
}
