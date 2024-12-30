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

namespace web.Controllers
{
    [Authorize]

    public class WatchlistController : Controller
    {
        private readonly BelezkaContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public WatchlistController(BelezkaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: Watchlist
        public async Task<IActionResult> Index()
        {

            var currentUser = await _usermanager.GetUserAsync(User);
            var watchlist = await _context.Watchlists.FirstOrDefaultAsync(n => n.OwnerId == currentUser);
            if (watchlist == null)
            {
                watchlist = new Watchlist { OwnerId = currentUser };

                _context.Watchlists.Add(watchlist);
                await _context.SaveChangesAsync();
            }
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

            var belezkaContext = _context.Watchlists.Include(w => w.OwnerId);
            return View(await belezkaContext.ToListAsync());

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

        public class WatchlistDetailsViewModel
        {
            public Watchlist Watchlist { get; set; }
            public List<Asset> Assets { get; set; }
            public List<Asset> Saved { get; set; }
        }

        // GET: Watchlist/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchlist = await _context.Watchlists
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (watchlist == null)
            {
                return NotFound();
            }


            // Preberi vse "Assets" iz baze
            var assets = await _context.Assets.ToListAsync();
            var saved = await _context.WatchlistAssets
            .Where(wa => wa.WatchlistId == id) // Filtriraj po WatchlistId
            .Include(wa => wa.Asset)           // Vključi povezano Asset entiteto
            .Select(wa => wa.Asset)            // Izberi samo Asset objekte
            .OrderBy(a => a.Id)              // Razvrsti po imenu
            .ToListAsync();
            // Ustvari ViewModel in dodeli podatke
            var viewModel = new WatchlistDetailsViewModel
            {
                Watchlist = watchlist,
                Assets = assets,
                Saved = saved
            };

            viewModel.Assets = await _context.Assets.ToListAsync(); // Pridobite vse assete iz baze
            var currentUser = await _usermanager.GetUserAsync(User);
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

            // Pošlji ViewModel na pogled
            return View(viewModel);
        }

        // GET: Watchlist/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Watchlist/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId")] Watchlist watchlist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(watchlist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", watchlist.UserId);
            return View(watchlist);
        }

        // GET: Watchlist/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            /*
            if (id == null)
            {
                return NotFound();
            }
            */
            var currentUser = await _usermanager.GetUserAsync(User);
            var watchlist = await _context.Watchlists.FirstOrDefaultAsync(n => n.OwnerId == currentUser);
            if (watchlist == null)
            {
                watchlist = new Watchlist { OwnerId = currentUser };

                _context.Watchlists.Add(watchlist);
                await _context.SaveChangesAsync();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", watchlist.UserId);
            return View(watchlist);
        }

        // POST: Watchlist/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId")] Watchlist watchlist)
        {
            if (id != watchlist.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(watchlist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WatchlistExists(watchlist.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", watchlist.UserId);
            return View(watchlist);
        }

        // GET: Watchlist/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var watchlist = await _context.Watchlists
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (watchlist == null)
            {
                return NotFound();
            }

            return View(watchlist);
        }

        // POST: Watchlist/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var watchlist = await _context.Watchlists.FindAsync(id);
            if (watchlist != null)
            {
                _context.Watchlists.Remove(watchlist);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WatchlistExists(int id)
        {
            return _context.Watchlists.Any(e => e.Id == id);
        }
    }
}
