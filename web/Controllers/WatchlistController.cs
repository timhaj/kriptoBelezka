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
                watchlist = new Watchlist{ OwnerId = currentUser};

                _context.Watchlists.Add(watchlist);
                await _context.SaveChangesAsync();
            }
            /*
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", watchlist.UserId);
            return View(watchlist);
            */
            
            var belezkaContext = _context.Watchlists.Include(w => w.OwnerId);
            return View(await belezkaContext.ToListAsync());
            
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

            return View(watchlist);
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
                watchlist = new Watchlist{ OwnerId = currentUser};

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
