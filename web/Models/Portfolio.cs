using System;
using System.Collections.Generic;

namespace web.Models;

public class Portfolio
{
    public int Id { get; set; }
    public int? UserId { get; set; }

    // Navigation properties
    public User? User { get; set; } // One-to-Many with User
    public ICollection<Transakcija>? Transactions { get; set; } // One-to-Many with Transactions
    public ICollection<PortfolioPerformance>? Performances { get; set; } // One-to-Many with PortfolioPerformance
}
