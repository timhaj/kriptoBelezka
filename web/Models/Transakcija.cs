using System;
using System.Collections.Generic;

namespace web.Models;

public class Transakcija
{
    public int Id { get; set; }
    public int? PortfolioId { get; set; }
    public int? AssetId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; } // 'kdaj'
    public decimal Price { get; set; } // 'cena'

    // Navigation properties
    public Portfolio? Portfolio { get; set; }
    public Asset? Asset { get; set; }
}
