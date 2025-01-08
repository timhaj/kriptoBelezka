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
using System.Formats.Tar;
using System.Globalization;

namespace web.Controllers
{
    public class TransakcijaController : Controller
    {
        private readonly BelezkaContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public TransakcijaController(BelezkaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: Transakcija
        public async Task<IActionResult> Index()
        {
            var belezkaContext = _context.Transakcijas.Include(t => t.Asset).Include(t => t.Portfolio);
            return View(await belezkaContext.ToListAsync());
        }

        // GET: Transakcija/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transakcija = await _context.Transakcijas
                .Include(t => t.Asset)
                .Include(t => t.Portfolio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transakcija == null)
            {
                return NotFound();
            }

            return View(transakcija);
        }

        // GET: Transakcija/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _usermanager.GetUserAsync(User);
            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.OwnerId == currentUser);
            var assets = await _context.Assets.ToListAsync();
            ViewBag.Assets = assets;
            ViewBag.PortId = portfolio;
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
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id");
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id");
            return View();
        }

        // POST: Transakcija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PortfolioId,AssetId,Quantity,Date,Price")] Transakcija transakcija)
        {
            string orderType = Request.Form["OrderType"].ToString();
            if (orderType == "Buy")
            {
                transakcija.Quantity = transakcija.Quantity;
            }
            else if (orderType == "Sell")
            {
                transakcija.Quantity = -transakcija.Quantity;
            }
            else
            {
                transakcija.Quantity = transakcija.Quantity;
            }
            if (ModelState.IsValid)
            {
                _context.Add(transakcija);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Portfolio");
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id", transakcija.AssetId);
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", transakcija.PortfolioId);
            return RedirectToAction("Index", "Portfolio");
            return View(transakcija);
        }

        // GET: Transakcija/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transakcija = await _context.Transakcijas.FindAsync(id);
            if (transakcija == null)
            {
                return NotFound();
            }
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id", transakcija.AssetId);
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", transakcija.PortfolioId);
            return View(transakcija);
        }

        // POST: Transakcija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PortfolioId,AssetId,Quantity,Date,Price")] Transakcija transakcija)
        {
            if (id != transakcija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transakcija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransakcijaExists(transakcija.Id))
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
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id", transakcija.AssetId);
            ViewData["PortfolioId"] = new SelectList(_context.Portfolios, "Id", "Id", transakcija.PortfolioId);
            return View(transakcija);
        }

        // GET: Transakcija/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transakcija = await _context.Transakcijas
                .Include(t => t.Asset)
                .Include(t => t.Portfolio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transakcija == null)
            {
                return NotFound();
            }

            return View(transakcija);
        }

        // POST: Transakcija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transakcija = await _context.Transakcijas.FindAsync(id);
            if (transakcija != null)
            {
                _context.Transakcijas.Remove(transakcija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransakcijaExists(int id)
        {
            return _context.Transakcijas.Any(e => e.Id == id);
        }
    }
}
