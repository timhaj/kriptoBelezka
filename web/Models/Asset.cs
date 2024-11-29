using System;
using System.Collections.Generic;

namespace web.Models;

public class Asset
{
    public int Id { get; set; }
    public string Name { get; set; } // 'ime'
    public decimal Price { get; set; }
    public decimal MarketCap { get; set; }
    public decimal CurrentSupply { get; set; }
    public decimal MaxSupply { get; set; } // Nullable if undefined
    public float ChangePercent24Hr { get; set; } // Change in 24h

    // Navigation properties
    public ICollection<WatchlistAsset>? WatchlistAssets { get; set; } // Many-to-Many
    public ICollection<Transakcija>? Transactions { get; set; } // Many-to-Many via Transakcija
}
