using ClientsManagmentAppExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClientsManagmentAppExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SetCookie([FromForm] CookieModel cookie)
        {
            await Task.Run(() =>
            {
                CookieOptions options = new()
                {
                    Expires = DateTime.Now.AddDays(cookie.Expires ??= 30),
                    Path = "/",
                    IsEssential = true

                };
                HttpContext.Response.Cookies.Append(cookie.Key, cookie.Value, options);
            });

            return Redirect(cookie.CurPath ?? "Index");
        }

        public IActionResult FatalError()
        {
            return View();
        }
    }
}