using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Controllers_Api
{
    [Route("api/v1/")]
    [ApiController]
    public class WatchlistApiController : ControllerBase
    {
        private readonly BelezkaContext _context;

        public WatchlistApiController(BelezkaContext context)
        {
            _context = context;
        }

        public class WatchlistDto
        {
            public int Id { get; set; }
            public string Email { get; set; }
        }

        public class AssetDto
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public decimal MarketCap { get; set; }
            public decimal CurrentSupply { get; set; }
            public decimal? MaxSupply { get; set; }
            public float ChangePercent24Hr { get; set; }
        }


        // GET: api/WatchlistApi
        [HttpGet("Watchlists")]
        public async Task<ActionResult<IEnumerable<WatchlistDto>>> GetWatchlists()
        {
            //return await _context.Watchlists.Include(w => w.OwnerId).ToListAsync();
            var watchlists = await _context.Watchlists
                .Include(w => w.OwnerId)
                .Select(w => new WatchlistDto
                {
                    Id = w.Id,
                    Email = w.OwnerId.Email
                })
                .ToListAsync();
            return watchlists;
        }

        /*         // GET: api/WatchlistApi/5
                [HttpGet("{id}")]
                public async Task<ActionResult<Watchlist>> GetWatchlist(int id)
                {
                    var watchlist = await _context.Watchlists.FindAsync(id);

                    if (watchlist == null)
                    {
                        return NotFound();
                    }

                    return watchlist;
                } */

        // GET: api/WatchlistApi/5
        [HttpGet("Watchlists/{id}")]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetWatchlist(int id)
        {
            var watchlist = await _context.Watchlists
                .Include(w => w.WatchlistAssets)
                .ThenInclude(wa => wa.Asset)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (watchlist == null)
            {
                return NotFound();
            }

            var assets = watchlist.WatchlistAssets
                .Select(wa => wa.Asset)
                .Where(a => a != null)
                .Select(a => new AssetDto
                {
                    Name = a.Name,
                    Price = a.Price,
                    MarketCap = a.MarketCap,
                    CurrentSupply = a.CurrentSupply,
                    MaxSupply = a.MaxSupply,
                    ChangePercent24Hr = a.ChangePercent24Hr
                });

            return Ok(assets);
        }

        /*         // PUT: api/WatchlistApi/5
                // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
                [HttpPut("{id}")]
                public async Task<IActionResult> PutWatchlist(int id, Watchlist watchlist)
                {
                    if (id != watchlist.Id)
                    {
                        return BadRequest();
                    }

                    _context.Entry(watchlist).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!WatchlistExists(id))
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

                // POST: api/WatchlistApi
                // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
                [HttpPost]
                public async Task<ActionResult<Watchlist>> PostWatchlist(Watchlist watchlist)
                {
                    _context.Watchlists.Add(watchlist);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetWatchlist", new { id = watchlist.Id }, watchlist);
                }

                // DELETE: api/WatchlistApi/5
                [HttpDelete("{id}")]
                public async Task<IActionResult> DeleteWatchlist(int id)
                {
                    var watchlist = await _context.Watchlists.FindAsync(id);
                    if (watchlist == null)
                    {
                        return NotFound();
                    }

                    _context.Watchlists.Remove(watchlist);
                    await _context.SaveChangesAsync();

                    return NoContent();
                } */

        private bool WatchlistExists(int id)
        {
            return _context.Watchlists.Any(e => e.Id == id);
        }
    }
}
