using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace web.Controllers
{
    [Authorize]
    public class NastavitveController : Controller
    {
        private readonly BelezkaContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public NastavitveController(BelezkaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
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

            var currentUser = await _usermanager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                nastavitve.OwnerId = currentUser;
                _context.Add(nastavitve);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nastavitve.UserId);
            return View(nastavitve);
        }

        private string GenerateApiKey()
        {
            string apiKey;
            do
            {
                using (var rng = new RNGCryptoServiceProvider())
                {
                    var byteArray = new byte[32];
                    rng.GetBytes(byteArray);
                    apiKey = Convert.ToBase64String(byteArray);
                }
            } while (_context.Nastavitves.Any(n => n.ApiKey == apiKey));
            return apiKey;
        }

        // GET: Nastavitve/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            /*             if (id == null) //uporabnik mora biti ze prijavljen, zato bo vedno imel Nastavitve/Edit (ker je [Authorize] zgoraj)
                        {
                            return NotFound();
                        } */

            var currentUser = await _usermanager.GetUserAsync(User);
            var nastavitve = await _context.Nastavitves.FirstOrDefaultAsync(n => n.OwnerId == currentUser);

            if (nastavitve == null)
            {
                nastavitve = new Nastavitve
                {
                    OwnerId = currentUser, // Assign the current user's ID
                    IsDarkMode = false,       // Default value
                    CurrentCurrencySelected = "united-states-dollar", // Default value
                    ApiKey = GenerateApiKey()
                };
                Console.WriteLine(nastavitve.ApiKey);
                _context.Nastavitves.Add(nastavitve);
                await _context.SaveChangesAsync();
                ViewBag.SavedCurrency = nastavitve;
                ViewBag.ApiKey = nastavitve.ApiKey;
            }
            else
            {
                ViewBag.SavedCurrency = nastavitve;
                ViewBag.ApiKey = nastavitve.ApiKey;
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nastavitve.UserId);
            if (currentUser != null)
            {
                var nastavitves = await _context.Nastavitves
                                            .Where(s => s.OwnerId == currentUser)
                                            .FirstOrDefaultAsync();
                if (nastavitves != null && nastavitves.IsDarkMode == true)
                {
                    ViewBag.mode = "dark";
                }
                else
                {
                    ViewBag.mode = "light";
                }
            }
            string apiUrl = "https://api.coincap.io/v2/rates";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStringAsync(apiUrl);
                    var apiResult = JsonConvert.DeserializeObject<ApiResponse>(response);
                    ViewBag.Rates = apiResult.Data;
                    //return View();
                }
                catch (Exception ex)
                {
                    // Handle any errors
                    ViewBag.Error = $"An error occurred: {ex.Message}";
                }
            }

            return View(nastavitve);
        }


        public class ApiResponse
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

        // POST: Nastavitve/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,IsDarkMode,CurrentCurrencySelected,ApiKey")] Nastavitve nastavitve)
        {
            nastavitve.IsDarkMode = !nastavitve.IsDarkMode;
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
                return RedirectToAction(nameof(Edit));
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
