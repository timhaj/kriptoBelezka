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
                    ProfitLoss = (g.Sum(t => (decimal)t.Quantity) * (decimal)g.First().Asset.Price) / (g.Sum(t => (decimal)t.Quantity) * g.Average(t => (decimal)t.Price)) - 1
                })
                .ToList();
                ViewBag.overview = transactionOverview;
                //total portfolio networth
                // Calculate portfolio net worth
                var portfolioNetWorth = saved
                    .GroupBy(t => t.Asset.Name) // Group by Asset Name
                    .Select(g => g.Sum(t => (decimal)t.Quantity * (decimal)t.Asset.Price)) // Calculate total value for each asset
                    .Sum(); // Sum up the total values of all assets
                ViewBag.PortfolioNetWorth = portfolioNetWorth;

                foreach (var t in saved)
                {//dobimo zadnji transaction
                    ViewBag.lastTransaction = t;
                }

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
            }
            return View(portfolios ?? new List<Portfolio>());
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
