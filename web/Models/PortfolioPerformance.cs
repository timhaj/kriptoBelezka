using System;
using System.Collections.Generic;

namespace web.Models;

public class PortfolioPerformance
{
    public int Id { get; set; }
    public int? PortfolioId { get; set; }
    public DateTime Date { get; set; }
    public float ChangePercent { get; set; }
    public decimal ChangeCurrency { get; set; }
    public decimal InitialInvestment { get; set; }

    // Navigation property
    public Portfolio? Portfolio { get; set; }
}
