using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SAGWeb.Data;
using SAGWeb.Models;
using SAGWeb.Services;
using SAGWeb.ViewModels;

namespace SAGWeb.Controllers
{
    public class CotizacionesController : Controller
    {
        private readonly SagrisaDbContext _context;
        private readonly IEmailService _emailService;

        public CotizacionesController(SagrisaDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string busqueda)
        {
            var vendedor = await _context.SagusuariosMovils
        .FirstOrDefaultAsync(v => v.Nombre == HttpContext.Session.GetString("NombreVendedor"));

            if (vendedor == null)
            {
                return Unauthorized();
            }

            var query = _context.Cotizaciones
                .Include(c => c.CodClienteNavigation)
                .Include(c => c.CodVendedorNavigation)
                .Where(c => c.CodVendedor == vendedor.CodVendedor);

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(c => c.CodClienteNavigation.NomCliente.Contains(busqueda));
            }

            var lista = await query
                .OrderByDescending(c => c.FechaHora)
                .Select(c => new CotizacionListadoViewModel
                {
                    CodCotizacion = c.CodCotizacion,
                    NombreCliente = c.CodClienteNavigation.NomCliente,
                    Fecha = (DateTime)c.FechaHora,
                    PrecioTotal = (decimal)c.PrecioTotal
                })
                .ToListAsync();

            return View(lista);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.CodClienteNavigation)
                .Include(c => c.CodVendedorNavigation)
                .Include(c => c.CotizacionDetalles)
                    .ThenInclude(d => d.CodProductoNavigation)
                .FirstOrDefaultAsync(c => c.CodCotizacion == id);

            if (cotizacion == null)
                return NotFound();

            var viewModel = new CotizacionDetalleViewModel
            {
                CodCotizacion = cotizacion.CodCotizacion,
                NombreCliente = cotizacion.CodClienteNavigation?.NomCliente,
                CorreoCliente = cotizacion.CodClienteNavigation?.Correo,
                NombreVendedor = cotizacion.CodVendedorNavigation?.Nombre,
                Fecha = (DateTime)cotizacion.FechaHora,
                Productos = cotizacion.CotizacionDetalles.Select(d => new CotizacionDetalleProductoViewModel
                {
                    Nombre = d.CodProductoNavigation?.NomProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList(),
                Total = (decimal)cotizacion.PrecioTotal
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Paso1()
        {
            return View(new ClienteWizardViewModel());
        }

        [HttpPost]
        public IActionResult Paso1(ClienteWizardViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Guardar en TempData o Session
            TempData["ClienteWizard"] = JsonConvert.SerializeObject(model);

            return RedirectToAction("Paso2");
        }

        [HttpGet]
        public JsonResult AutocompletarCliente(string term)
        {
            var resultados = _context.Clientes
                .Where(c => c.NomCliente.Contains(term))
                .Select(c => new
                {
                    label = c.NomCliente,
                    value = c.NomCliente
                })
                .Take(10)
                .ToList();

            return Json(resultados);
        }

        [HttpGet]
        public JsonResult ObtenerDatosCliente(string nombre)
        {
            var cliente = _context.Clientes
                .Where(c => c.NomCliente == nombre)
                .Select(c => new {
                    c.NomCliente,
                    Empresa = "",
                    Departamento = "",
                    Municipio = "",
                    Direccion = c.Ciudad,
                    c.Correo,
                    Telefono = ""
                })
                .FirstOrDefault();

            return Json(cliente);
        }

        [HttpGet]
        public JsonResult AutocompletarProducto(string term)
        {
            var productos = _context.SagpreciosEnLineas
                .Where(p => p.NomProducto.Contains(term))
                .Select(p => new { label = p.NomProducto, value = p.NomProducto })
                .Take(10)
                .ToList();

            return Json(productos);
        }

        [HttpGet]
        public JsonResult ObtenerDatosProducto(string nombre)
        {
            var producto = _context.SagpreciosEnLineas
                .Where(p => p.NomProducto == nombre)
                .Select(p => new {
                    p.CodProducto,
                    p.Bodega,
                    Existencia = p.Existencia,
                    PrecioBase = p.Pbase
                })
                .FirstOrDefault();

            return Json(producto);
        }

        [HttpGet]
        public IActionResult Paso2()
        {
            return View(new ProductoWizardViewModel());
        }

        [HttpPost]
        public IActionResult Paso2(string productosJson)
        {
            if (string.IsNullOrEmpty(productosJson))
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto.");
                return View(new ProductoWizardViewModel());
            }

            TempData["ProductosCotizacion"] = productosJson;

            return RedirectToAction("Paso3");
        }

        [HttpGet]
        public IActionResult Paso3()
        {
            TempData.Keep("ProductosCotizacion");
            TempData.Keep("ClienteWizard");

            var productosJson = TempData["ProductosCotizacion"] as string;
            var clienteJson = TempData["ClienteWizard"] as string;

            if (string.IsNullOrEmpty(productosJson) || string.IsNullOrEmpty(clienteJson))
            {
                return RedirectToAction("Paso1");
            }

            var productos = JsonConvert.DeserializeObject<List<ProductoDetalleViewModel>>(productosJson);
            var cliente = JsonConvert.DeserializeObject<ClienteWizardViewModel>(clienteJson);

            if (productos == null || productos.Count == 0)
            {
                TempData["Error"] = "No se pudieron cargar los productos. Reintente.";
                return RedirectToAction("Paso2");
            }

            var model = new CotizacionResumenViewModel
            {
                FechaCotizacion = DateTime.Now.ToString("dd/MM/yyyy"),
                NombreCliente = cliente.Nombre,
                Empresa = cliente.Empresa,
                CorreoCliente = cliente.Correo,
                Productos = productos
            };

            HttpContext.Session.SetString("ResumenCotizacion", JsonConvert.SerializeObject(model));
            TempData.Keep();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Paso4()
        {
            var resumenJson = HttpContext.Session.GetString("ResumenCotizacion");
            if (string.IsNullOrEmpty(resumenJson))
                return RedirectToAction("Paso1");


            var resumen = JsonConvert.DeserializeObject<CotizacionResumenViewModel>(resumenJson);

            if (resumen == null)
            {
                return BadRequest("Error al deserializar el resumen de la cotización.");
            }

            // Guardar en base de datos
            var cliente = _context.Clientes.FirstOrDefault(c => c.Correo == resumen.CorreoCliente);

            if (cliente == null)
            {
                return BadRequest("No se encontró el cliente con el correo proporcionado.");
            }

            var vendedor = _context.SagusuariosMovils.FirstOrDefault(v => v.Nombre == HttpContext.Session.GetString("NombreVendedor"));
            
            if (vendedor == null)
            {
                return Unauthorized("El usuario no ha iniciado sesión correctamente.");
            }


            var cotizacion = new Cotizacione
            {
                CodCliente = cliente.CodCliente,
                CodVendedor = vendedor.CodVendedor,
                FechaHora = DateTime.Now,
                PrecioTotal = resumen.Total
            };

            _context.Cotizaciones.Add(cotizacion);
            await _context.SaveChangesAsync();

            foreach (var prod in resumen.Productos)
            {
                var productoDb = _context.SagpreciosEnLineas.FirstOrDefault(p => p.NomProducto == prod.NombreProducto);
                if (productoDb == null) continue;

                var detalle = new CotizacionDetalle
                {
                    CodCotizacion = cotizacion.CodCotizacion,
                    CodProducto = productoDb.CodProducto,
                    NombreProducto = prod.NombreProducto,
                    Cantidad = prod.Cantidad,
                    PrecioUnitario = (decimal)prod.Precio,
                    Subtotal = (decimal?)prod.Subtotal
                };
                _context.CotizacionDetalles.Add(detalle);
            }

            await _context.SaveChangesAsync();

            // Enviar correo
            var body = new StringBuilder();
            body.AppendLine($"Estimado {resumen.NombreCliente},<br><br>");
            body.AppendLine("Le compartimos su cotización:<br><br>");
            body.AppendLine("<table border='1' cellpadding='5'><tr><th>Producto</th><th>Cantidad</th><th>Precio</th><th>Subtotal</th></tr>");

            foreach (var item in resumen.Productos)
            {
                body.AppendLine($"<tr><td>{item.NombreProducto}</td><td>{item.Cantidad}</td><td>${item.Precio:F2}</td><td>${item.Subtotal:F2}</td></tr>");
            }

            body.AppendLine("</table><br>");
            body.AppendLine($"<b>Subtotal:</b> ${resumen.Subtotal:F2}<br>");
            body.AppendLine($"<b>IVA:</b> ${resumen.IVA:F2}<br>");
            body.AppendLine($"<b>Total:</b> ${resumen.Total:F2}<br><br>");
            body.AppendLine("Gracias por confiar en SAGRISA.");

            await _emailService.SendEmailAsync(resumen.CorreoCliente, "Cotización SAGRISA", body.ToString());

            return View("Confirmacion");
        }
    }
}
