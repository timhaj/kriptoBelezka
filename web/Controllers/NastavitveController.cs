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
    public class NastavitveController : Controller
    {
        private readonly BelezkaContext _context;

        public NastavitveController(BelezkaContext context)
        {
            _context = context;
        }

        // GET: Nastavitve
        public async Task<IActionResult> Index()
        {
            var belezkaContext = _context.Nastavitves.Include(n => n.User);
            return View(await belezkaContext.ToListAsync());
        }

        // GET: Nastavitve/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nastavitve = await _context.Nastavitves
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nastavitve == null)
            {
                return NotFound();
            }

            return View(nastavitve);
        }

        // GET: Nastavitve/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Nastavitve/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,IsDarkMode,CurrentCurrencySelected")] Nastavitve nastavitve)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nastavitve);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nastavitve.UserId);
            return View(nastavitve);
        }

        // GET: Nastavitve/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nastavitve = await _context.Nastavitves.FindAsync(id);
            if (nastavitve == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nastavitve.UserId);
            return View(nastavitve);
        }

        // POST: Nastavitve/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,IsDarkMode,CurrentCurrencySelected")] Nastavitve nastavitve)
        {
            if (id != nastavitve.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nastavitve);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NastavitveExists(nastavitve.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nastavitve.UserId);
            return View(nastavitve);
        }

        // GET: Nastavitve/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nastavitve = await _context.Nastavitves
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nastavitve == null)
            {
                return NotFound();
            }

            return View(nastavitve);
        }

        // POST: Nastavitve/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nastavitve = await _context.Nastavitves.FindAsync(id);
            if (nastavitve != null)
            {
                _context.Nastavitves.Remove(nastavitve);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NastavitveExists(int id)
        {
            return _context.Nastavitves.Any(e => e.Id == id);
        }
    }
}
