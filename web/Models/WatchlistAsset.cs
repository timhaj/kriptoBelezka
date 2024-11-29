using System;
using System.Collections.Generic;

namespace web.Models;

public class WatchlistAsset
{
    public int Id { get; set; }
    public int? WatchlistId { get; set; }
    public int? AssetId { get; set; }

    // Navigation properties
    public Watchlist? Watchlist { get; set; }
    public Asset? Asset { get; set; }
}
