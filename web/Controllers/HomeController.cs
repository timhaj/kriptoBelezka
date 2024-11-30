using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        
        string apiUrl = "https://api.coincap.io/v2/assets?limit=20";

        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetStringAsync(apiUrl);
            var apiResult = JsonConvert.DeserializeObject<ApiResponse>(response);

            // Pass the data to the view
            return View(apiResult.Data);
        }
        
        return View();
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
