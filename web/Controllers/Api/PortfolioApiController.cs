using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using web.Filters;

namespace web.Controllers_Api
{
    [Route("api/v1")]
    [ApiController]
    /* [ApiKeyAuth] */
    public class PortfolioApiController : ControllerBase
    {
        private readonly BelezkaContext _context;

        public PortfolioApiController(BelezkaContext context)
        {
            _context = context;
        }

        public class TransactionDto
        {
            public DateTime Date { get; set; }
            public string AssetName { get; set; }
            public decimal Price { get; set; }
            public decimal Quantity { get; set; }
            public string OrderType { get; set; } // BUY or SELL
        }

        public class AssetOverviewDto
        {
            public string AssetName { get; set; }
            public decimal TotalQuantity { get; set; }
            public decimal AveragePrice { get; set; }
            public decimal CurrentAssetPrice { get; set; }
            public decimal ProfitLoss { get; set; }
        }

        public class PortfolioNetWorthDto
        {
            public decimal NetWorth { get; set; }
        }

        /*         // GET: api/PortfolioApi
                [HttpGet]
                public async Task<ActionResult<IEnumerable<Portfolio>>> GetPortfolios()
                {
                    return await _context.Portfolios.ToListAsync();
                } */

        /*         // GET: api/PortfolioApi/5
                [HttpGet("{id}")]
                public async Task<ActionResult<Portfolio>> GetPortfolio(int id)
                {
                    var portfolio = await _context.Portfolios.FindAsync(id);

                    if (portfolio == null)
                    {
                        return NotFound();
                    }

                    return portfolio;
                } */


        // GET: api/PortfolioApi/5
        [HttpGet("Transactions/{id}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetPortfolio(string id)
        {
            var userSettings = await _context.Nastavitves.Include(n => n.OwnerId)
                .FirstOrDefaultAsync(n => n.ApiKey.Equals(id));
            if (userSettings == null)
            {
                return NotFound("Invalid API key.");
            }


            var portfolio = await _context.Portfolios
                .Include(p => p.Transactions)
                .ThenInclude(t => t.Asset)
                .FirstOrDefaultAsync(p => p.OwnerId.Id == userSettings.OwnerId.Id);

            if (portfolio == null)
            {
                return NotFound();
            }

            var transactions = portfolio.Transactions
                .Select(t => new TransactionDto
                {
                    Date = t.Date,
                    AssetName = t.Asset.Name,
                    Price = t.Price,
                    Quantity = t.Quantity,
                    OrderType = t.Quantity >= 0 ? "BUY" : "SELL"
                });

            return Ok(transactions);
        }

        // GET: api/v1/Overview/{id}
        [HttpGet("Overview/{id}")]
        public async Task<ActionResult<IEnumerable<AssetOverviewDto>>> GetPortfolioOverview(string id)
        {
            var userSettings = await _context.Nastavitves.Include(n => n.OwnerId)
                .FirstOrDefaultAsync(n => n.ApiKey.Equals(id));
            if (userSettings == null)
            {
                return NotFound("Invalid API key.");
            }

            var transactions = await _context.Transakcijas
                .Include(t => t.Portfolio)
                .Include(t => t.Asset)
                .Where(t => t.Portfolio.OwnerId.Id == userSettings.OwnerId.Id)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound();
            }

            var transactionOverview = transactions
                .GroupBy(t => t.Asset.Name)
                .Select(g => new AssetOverviewDto
                {
                    AssetName = g.Key,
                    TotalQuantity = g.Sum(t => t.Quantity),
                    AveragePrice = g.Average(t => t.Price),
                    CurrentAssetPrice = g.First().Asset.Price,
                    ProfitLoss = ((g.Sum(t => t.Quantity) * g.First().Asset.Price) /
                                 ((g.Sum(t => t.Quantity) * g.Average(t => t.Price)) + 0.01m)) - 1
                })
                .Where(g => g.TotalQuantity > 0)
                .ToList();

            return Ok(transactionOverview);
        }

        [HttpGet("Networth/{id}")]
        public async Task<ActionResult<decimal>> GetPortfolioValue(string id)
        {
            var userSettings = await _context.Nastavitves.Include(n => n.OwnerId)
                .FirstOrDefaultAsync(n => n.ApiKey.Equals(id));
            if (userSettings == null)
            {
                return NotFound("Invalid API key.");
            }
            var transactions = await _context.Transakcijas
                .Include(t => t.Portfolio)
                .Include(t => t.Asset)
                .Where(t => t.Portfolio.OwnerId.Id == userSettings.OwnerId.Id)
                .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound();
            }

            var portfolioNetWorth = transactions
                .GroupBy(t => t.Asset.Name)
                .Select(g => g.Sum(t => t.Quantity * t.Asset.Price))
                .Sum();

            var result = new PortfolioNetWorthDto { NetWorth = portfolioNetWorth };
            return Ok(result);
        }

        /*         // PUT: api/PortfolioApi/5
                // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
                [HttpPut("{id}")]
                public async Task<IActionResult> PutPortfolio(int id, Portfolio portfolio)
                {
                    if (id != portfolio.Id)
                    {
                        return BadRequest();
                    }

                    _context.Entry(portfolio).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PortfolioExists(id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return NoContent();
                }

                // POST: api/PortfolioApi
                // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
                [HttpPost]
                public async Task<ActionResult<Portfolio>> PostPortfolio(Portfolio portfolio)
                {
                    _context.Portfolios.Add(portfolio);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetPortfolio", new { id = portfolio.Id }, portfolio);
                }

                // DELETE: api/PortfolioApi/5
                [HttpDelete("{id}")]
                public async Task<IActionResult> DeletePortfolio(int id)
                {
                    var portfolio = await _context.Portfolios.FindAsync(id);
                    if (portfolio == null)
                    {
                        return NotFound();
                    }

                    _context.Portfolios.Remove(portfolio);
                    await _context.SaveChangesAsync();

                    return NoContent();
                } */

        private bool PortfolioExists(int id)
        {
            return _context.Portfolios.Any(e => e.Id == id);
        }
    }
}
