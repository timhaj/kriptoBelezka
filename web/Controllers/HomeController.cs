using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace web.Controllers;

public class HomeController : Controller
{
    private readonly BelezkaContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _usermanager;

    public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, BelezkaContext context)
    {
        _logger = logger;
        _usermanager = userManager;
        _context = context;
    }


    public async Task<IActionResult> Index()
    {

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
                        //Console.WriteLine(matchingRate);
                        ViewBag.CurrentRate = matchingRate.RateUsd;
                        ViewBag.CurrentSymbol = matchingRate.CurrencySymbol;
                        ViewBag.CurSymbol = matchingRate.Symbol;
                        //Console.WriteLine(apiResultRates);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = $"An error occurred: {ex.Message}";
                    }
                }
                //Console.WriteLine(nastavitve.CurrentCurrencySelected);
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

            // Pass the data to the view
            return View(apiResult.Data);
        }

        return View();
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























    // Helper classes to map the API response
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




    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
