using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using EFCore.BulkExtensions;
using System.Globalization;
using System.IO;
using System.Text;

namespace web.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly BelezkaContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public PortfolioController(BelezkaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        public IActionResult Export()
        {
            var transactions = _context.Transakcijas
                .Include(t => t.Asset)
                .ToList();
            var csvString = GenerateCsv(transactions);
            var bytes = Encoding.UTF8.GetBytes(csvString);
            var result = new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = "transactions.csv"
            };
            return result;
        }

        private string GenerateCsv(IEnumerable<Transakcija> transactions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("AssetName,Quantity,Date,Price,OrderType");
            foreach (var t in transactions)
            {
                var assetName = t.Asset?.Name ?? "N/A";
                var orderType = t.Quantity >= 0 ? "BUY" : "SELL";
                sb.AppendLine($"{assetName},{t.Quantity.ToString(CultureInfo.InvariantCulture)},{t.Date.ToString("yyyy-MM-dd")},{t.Price.ToString(CultureInfo.InvariantCulture)},{orderType}");
            }
            return sb.ToString();
        }

        // GET: Portfolio
        public async Task<IActionResult> Index()
        {
            var currentUser = await _usermanager.GetUserAsync(User);
            var portfolios = await _context.Portfolios
                .Where(n => n.OwnerId == currentUser)
                .ToListAsync();
            // transaction history
            var portfolioID = await _context.Portfolios.FirstOrDefaultAsync(p => p.OwnerId == currentUser);
            if (portfolioID != null)
            {
                var saved = await _context.Transakcijas
                .Where(wa => wa.PortfolioId == portfolioID.Id)
                .Include(p => p.Asset)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
                ViewBag.transactions = saved;
                // overview data
                var transactionOverview = saved
                .GroupBy(t => t.Asset.Name) // Group by Asset Name
                .Select(g => new
                {
                    AssetName = g.Key, // The name of the asset
                    TotalQuantity = g.Sum(t => t.Quantity), // Sum of quantities for this asset
                    AveragePrice = g.Average(t => t.Price), // Average price for this asset
                    CurrentAssetPrice = g.First().Asset.Price,
                    ProfitLoss = (g.Sum(t => (decimal)t.Quantity) * (decimal)g.First().Asset.Price) / (g.Sum(t => (decimal)t.Quantity) * g.Average(t => (decimal)t.Price) + 0.01m) - 1
                })
                .Where(g => g.TotalQuantity > 0)
                .ToList();
                ViewBag.overview = transactionOverview;
                //total portfolio networth
                // Calculate portfolio net worth
                var portfolioNetWorth = saved
                    .GroupBy(t => t.Asset.Name) // Group by Asset Name
                    .Select(g => g.Sum(t => (decimal)t.Quantity * (decimal)t.Asset.Price)) // Calculate total value for each asset
                    .Sum(); // Sum up the total values of all assets
                ViewBag.PortfolioNetWorth = portfolioNetWorth;
                ViewBag.lastTransaction = DateTime.Now.AddDays(-1);
                foreach (var t in saved)
                {//dobimo zadnji transaction
                    ViewBag.lastTransaction = t;
                }
            }
            else
            {
                ViewBag.PortfolioNetWorth = -1;
                ViewBag.lastTransaction = DateTime.Now.AddDays(-1);
                ViewBag.overview = null;
                ViewBag.transactions = null;
            }

            //user id
            ViewBag.UserId = currentUser.Id;
            if (currentUser != null)
            {
                var nastavitve = await _context.Nastavitves
                                            .Where(s => s.OwnerId == currentUser)
                                            .FirstOrDefaultAsync();
                if (nastavitve != null && nastavitve.IsDarkMode == true)
                {
                    ViewBag.mode = "dark";
                }
                else
                {
                    ViewBag.mode = "light";
                }
                if (nastavitve != null)
                {
                    string ratesAPI = "https://api.coincap.io/v2/rates";
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            var response = await client.GetStringAsync(ratesAPI);
                            var apiResultRates = JsonConvert.DeserializeObject<ApiResponse2>(response);
                            var matchingRate = apiResultRates?.Data.FirstOrDefault(rate => rate.Id.Equals(nastavitve.CurrentCurrencySelected, StringComparison.OrdinalIgnoreCase));
                            ViewBag.CurrentRate = matchingRate.RateUsd;
                            ViewBag.CurrentSymbol = matchingRate.CurrencySymbol.Trim();
                            ViewBag.CurSymbol = matchingRate.Symbol;
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Error = $"An error occurred: {ex.Message}";
                        }
                    }
                }
                else
                {
                    ViewBag.CurrentRate = 1;
                    ViewBag.CurrentSymbol = "$";
                    ViewBag.CurSymbol = "USD";
                    ViewBag.mode = "light";
                }
            }
            else
            {
                ViewBag.CurrentRate = 1;
                ViewBag.CurrentSymbol = "$";
                ViewBag.CurSymbol = "USD";
                ViewBag.mode = "light";
            }
            string apiUrl = "https://api.coincap.io/v2/assets?limit=2000";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(apiUrl);
                var apiResult = JsonConvert.DeserializeObject<ApiResponse>(response);

                var newAssets = apiResult.Data.Select(asset => new Asset
                {
                    Name = asset.Name,
                    Price = ParseDecimal(asset.PriceUsd, 0), // Default to 0 if parsing fails
                    MarketCap = ParseDecimal(asset.MarketCapUsd, 0),
                    CurrentSupply = ParseDecimal(asset.Supply, 0),
                    MaxSupply = ParseDecimal(asset.MaxSupply, 0),
                    ChangePercent24Hr = (float)ParseDecimal(asset.ChangePercent24Hr, 0)
                }).ToList();

                // Loči nove in obstoječe zapise
                var existingAssets = _context.Assets.AsNoTracking().ToList();
                var assetsToUpdate = existingAssets.Where(e => newAssets.Any(n => n.Name == e.Name)).ToList();
                var assetsToAdd = newAssets.Where(n => !existingAssets.Any(e => e.Name == n.Name)).ToList();

                // Posodobi obstoječe zapise
                foreach (var existing in assetsToUpdate)
                {
                    var updated = newAssets.First(n => n.Name == existing.Name);
                    existing.Price = updated.Price;
                    existing.MarketCap = updated.MarketCap;
                    existing.CurrentSupply = updated.CurrentSupply;
                    existing.MaxSupply = updated.MaxSupply;
                    existing.ChangePercent24Hr = updated.ChangePercent24Hr;
                }

                // Shrani vse spremembe
                await _context.BulkUpdateAsync(assetsToUpdate);
                await _context.BulkInsertAsync(assetsToAdd);
            }
            return View(portfolios ?? new List<Portfolio>());
        }


        decimal ParseDecimal(string input, decimal defaultValue)
        {
            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        public class ApiResponse
        {
            public List<CryptoData> Data { get; set; }
        }

        public class CryptoData
        {
            public string Rank { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
            public string PriceUsd { get; set; }
            public string Supply { get; set; }
            public string MaxSupply { get; set; }
            public string MarketCapUsd { get; set; }
            public string VolumeUsd24Hr { get; set; }
            public string ChangePercent24Hr { get; set; }
        }


        public class ApiResponse2
        {
            public Rate[] Data { get; set; }
        }

        public class Rate
        {
            public string Id { get; set; }
            public string Symbol { get; set; }
            public string CurrencySymbol { get; set; }
            public string Type { get; set; }
            public string RateUsd { get; set; }
        }



        // GET: Portfolio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolio = await _context.Portfolios
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolio == null)
            {
                return NotFound();
            }

            return View(portfolio);
        }

        // GET: Portfolio/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Portfolio/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId")] Portfolio portfolio)
        {
            // Access the OwnerId from the form
            var ownerId = Request.Form["OwnerId"].ToString();
            var currentUser = await _usermanager.GetUserAsync(User);
            //var owner = await _context.ApplicationUser.FindAsync(ownerId);
            var portfolios = new Portfolio { OwnerId = currentUser };
            _context.Portfolios.Add(portfolios);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", portfolio.UserId);
            return View(portfolio);
        }

        // GET: Portfolio/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", portfolio.UserId);
            return View(portfolio);
        }

        // POST: Portfolio/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId")] Portfolio portfolio)
        {
            if (id != portfolio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(portfolio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PortfolioExists(portfolio.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", portfolio.UserId);
            return View(portfolio);
        }

        // GET: Portfolio/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolio = await _context.Portfolios
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolio == null)
            {
                return NotFound();
            }

            return View(portfolio);
        }

        // POST: Portfolio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio != null)
            {
                _context.Portfolios.Remove(portfolio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioExists(int id)
        {
            return _context.Portfolios.Any(e => e.Id == id);
        }
    }
}
