using Microsoft.AspNetCore.Mvc;
using SAGWeb.Data;
using SAGWeb.ViewModels;

namespace SAGWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly SagrisaDbContext _context;

        public LoginController(SagrisaDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var vendedor = _context.SagusuariosMovils
                .FirstOrDefault(v => v.Nombre == model.Nombre && v.Pin == model.Pin);

            if (vendedor != null)
            {
                HttpContext.Session.SetInt32("CodVendedor", vendedor.CodVendedor);
                HttpContext.Session.SetString("NombreVendedor", vendedor.Nombre);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Credenciales inválidas.";
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
