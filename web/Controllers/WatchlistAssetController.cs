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
    public class WatchlistAssetController : Controller
    {
        private readonly BelezkaContext _context;

        public WatchlistAssetController(BelezkaContext context)
        {
            _context = context;
        }

        // GET: WatchlistAsset
        public async Task<IActionResult> Index()
        {
            var belezkaContext = _context.WatchlistAssets.Include(w => w.Asset).Include(w => w.Watchlist);
            return View(await belezkaContext.ToListAsync());
        }

        // GET: WatchlistAsset/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchlistAsset = await _context.WatchlistAssets
                .Include(w => w.Asset)
                .Include(w => w.Watchlist)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (watchlistAsset == null)
            {
                return NotFound();
            }

            return View(watchlistAsset);
        }

        // GET: WatchlistAsset/Create
        public IActionResult Create()
        {
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id");
            ViewData["WatchlistId"] = new SelectList(_context.Watchlists, "Id", "Id");
            return View();
        }

        // POST: WatchlistAsset/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,WatchlistId,AssetId")] WatchlistAsset watchlistAsset)
        {
            if (ModelState.IsValid)
            {
                _context.Add(watchlistAsset);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id", watchlistAsset.AssetId);
            ViewData["WatchlistId"] = new SelectList(_context.Watchlists, "Id", "Id", watchlistAsset.WatchlistId);
            return View(watchlistAsset);
        }

        // GET: WatchlistAsset/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchlistAsset = await _context.WatchlistAssets.FindAsync(id);
            if (watchlistAsset == null)
            {
                return NotFound();
            }
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id", watchlistAsset.AssetId);
            ViewData["WatchlistId"] = new SelectList(_context.Watchlists, "Id", "Id", watchlistAsset.WatchlistId);
            return View(watchlistAsset);
        }

        // POST: WatchlistAsset/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WatchlistId,AssetId")] WatchlistAsset watchlistAsset)
        {
            if (id != watchlistAsset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(watchlistAsset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WatchlistAssetExists(watchlistAsset.Id))
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
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "Id", watchlistAsset.AssetId);
            ViewData["WatchlistId"] = new SelectList(_context.Watchlists, "Id", "Id", watchlistAsset.WatchlistId);
            return View(watchlistAsset);
        }

        // GET: WatchlistAsset/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchlistAsset = await _context.WatchlistAssets
                .Include(w => w.Asset)
                .Include(w => w.Watchlist)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (watchlistAsset == null)
            {
                return NotFound();
            }

            return View(watchlistAsset);
        }

        // POST: WatchlistAsset/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var watchlistAsset = await _context.WatchlistAssets.FindAsync(id);
            if (watchlistAsset != null)
            {
                _context.WatchlistAssets.Remove(watchlistAsset);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WatchlistAssetExists(int id)
        {
            return _context.WatchlistAssets.Any(e => e.Id == id);
        }
    }
}
