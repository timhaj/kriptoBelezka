using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Controllers
{
    public class PortfolioPerformanceController : Controller
    {
        private readonly BelezkaContext _context;

        public PortfolioPerformanceController(BelezkaContext context)
        {
            _context = context;
        }

        // GET: PortfolioPerformance
        public async Task<IActionResult> Index()
        {
            var belezkaContext = _context.PortfolioPerformances.Include(p => p.Portfolio);
            return View(await belezkaContext.ToListAsync());
        }

        // GET: PortfolioPerformance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioPerformance = await _context.PortfolioPerformances
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolioPerformance == null)
            {
                return NotFound();
            }

            return View(portfolioPerformance);
        }

        // GET: PortfolioPerformance/Create
        public IActionResult Create()
        {
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id");
            return View();
        }

        // POST: PortfolioPerformance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PortfolioId,Date,ChangePercent,ChangeCurrency,InitialInvestment")] PortfolioPerformance portfolioPerformance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(portfolioPerformance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", portfolioPerformance.PortfolioId);
            return View(portfolioPerformance);
        }

        // GET: PortfolioPerformance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioPerformance = await _context.PortfolioPerformances.FindAsync(id);
            if (portfolioPerformance == null)
            {
                return NotFound();
            }
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", portfolioPerformance.PortfolioId);
            return View(portfolioPerformance);
        }

        // POST: PortfolioPerformance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PortfolioId,Date,ChangePercent,ChangeCurrency,InitialInvestment")] PortfolioPerformance portfolioPerformance)
        {
            if (id != portfolioPerformance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(portfolioPerformance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PortfolioPerformanceExists(portfolioPerformance.Id))
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
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", portfolioPerformance.PortfolioId);
            return View(portfolioPerformance);
        }

        // GET: PortfolioPerformance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioPerformance = await _context.PortfolioPerformances
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (portfolioPerformance == null)
            {
                return NotFound();
            }

            return View(portfolioPerformance);
        }

        // POST: PortfolioPerformance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolioPerformance = await _context.PortfolioPerformances.FindAsync(id);
            if (portfolioPerformance != null)
            {
                _context.PortfolioPerformances.Remove(portfolioPerformance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioPerformanceExists(int id)
        {
            return _context.PortfolioPerformances.Any(e => e.Id == id);
        }
    }
}
