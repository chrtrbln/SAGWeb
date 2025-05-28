using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SAGWeb.Models;

namespace SAGWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("CodVendedor") == null)
                return RedirectToAction("Index", "Login");

            return View();
        }

        public IActionResult Privacy()
        {
            if (!UsuarioAutenticado)
                return RedirectToAction("Index", "Login");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
