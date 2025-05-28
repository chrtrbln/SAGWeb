using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGWeb.Data;
using SAGWeb.Models;

namespace SAGWeb.Controllers
{
    public class ClientesController : Controller
    {
        private readonly SagrisaDbContext _context;

        public ClientesController(SagrisaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Crear()
        {
            if (HttpContext.Session.GetInt32("CodVendedor") == null)
                return RedirectToAction("Index", "Login");

            return View();
        }

        [HttpPost]
        public IActionResult Crear(Cliente cliente)
        {
            if (HttpContext.Session.GetInt32("CodVendedor") == null)
                return RedirectToAction("Index", "Login");

            if (ModelState.IsValid)
            {
                cliente.Vendedor = HttpContext.Session.GetInt32("CodVendedor").Value;
                try
                {
                    _context.Clientes.Add(cliente);
                    _context.SaveChanges();
                    return RedirectToAction("Index","Home");
                }
                catch (DbUpdateException ex)
                {
                    var innerMessage = ex.InnerException?.Message;
                    ModelState.AddModelError("", $"Error al guardar en la base de datos: {innerMessage}");
                    return View(cliente);
                }
            }

            return View(cliente);
        }

        // API para autocompletado
        [HttpGet]
        public JsonResult BuscarPorNombre(string term)
        {
            var coincidencias = _context.Clientes
                .Where(c => c.NomCliente.Contains(term))
                .Select(c => new
                {
                    label = c.NomCliente,
                    value = c.CodCliente
                }).ToList();

            return Json(coincidencias);
        }

        // API para traer datos del cliente por ID
        [HttpGet]
        public JsonResult ObtenerDatosCliente(int codCliente)
        {
            var cliente = _context.Clientes
                .Where(c => c.CodCliente == codCliente)
                .Select(c => new
                {
                    c.NomCliente,
                    c.Correo,
                    c.Ciudad
                }).FirstOrDefault();

            return Json(cliente);
        }
    }
}
