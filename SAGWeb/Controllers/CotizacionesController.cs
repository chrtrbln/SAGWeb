using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SAGWeb.Data;
using SAGWeb.Models;
using SAGWeb.Services;
using SAGWeb.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

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
                    PrecioTotal = (decimal)c.PrecioTotal,
                    Estado = c.Estado ?? "Pendiente" // Incluir el estado
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
                Estado = cotizacion.Estado ?? "Pendiente", // Valor por defecto si es null
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

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            try
            {
                var estadosValidos = new[] { "Pendiente", "Aceptada", "Rechazada" };
                if (!estadosValidos.Contains(nuevoEstado))
                {
                    TempData["Error"] = "Estado no válido.";
                    return RedirectToAction("Detalle", new { id });
                }

                var cotizacion = await _context.Cotizaciones
                    .Include(c => c.CodClienteNavigation)
                    .FirstOrDefaultAsync(c => c.CodCotizacion == id);

                if (cotizacion == null)
                {
                    TempData["Error"] = "Cotización no encontrada.";
                    return RedirectToAction("Index");
                }

                var vendedor = await _context.SagusuariosMovils
                    .FirstOrDefaultAsync(v => v.Nombre == HttpContext.Session.GetString("NombreVendedor"));

                if (vendedor == null || cotizacion.CodVendedor != vendedor.CodVendedor)
                {
                    return Unauthorized();
                }

                cotizacion.Estado = nuevoEstado;

                await _context.SaveChangesAsync();

                if (nuevoEstado == "Aceptada" || nuevoEstado == "Rechazada")
                {
                    await EnviarNotificacionCambioEstado(cotizacion, nuevoEstado);
                }

                TempData["Success"] = $"Estado cambiado a '{nuevoEstado}' exitosamente.";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cambiar el estado: " + ex.Message;
                return RedirectToAction("Detalle", new { id });
            }
        }

        private async Task EnviarNotificacionCambioEstado(Cotizacione cotizacion, string nuevoEstado)
        {
            try
            {
                if (string.IsNullOrEmpty(cotizacion.CodClienteNavigation?.Correo))
                    return;

                var asunto = $"Actualización de Cotización #{cotizacion.CodCotizacion} - SAGRISA";
                var mensaje = new StringBuilder();

                mensaje.AppendLine($"Estimado/a {cotizacion.CodClienteNavigation.NomCliente},<br><br>");
                mensaje.AppendLine($"Le informamos que su cotización #{cotizacion.CodCotizacion} ha sido <strong>{nuevoEstado.ToLower()}</strong>.<br><br>");

                if (nuevoEstado == "Aceptada")
                {
                    mensaje.AppendLine("Nos complace informarle que su cotización ha sido aceptada. Nos pondremos en contacto con usted para coordinar los próximos pasos.<br><br>");
                }
                else if (nuevoEstado == "Rechazada")
                {
                    mensaje.AppendLine("Lamentamos informarle que su cotización no ha sido aprobada en esta ocasión. Para más información, puede contactarnos directamente.<br><br>");
                }

                mensaje.AppendLine($"<b>Fecha de cotización:</b> {cotizacion.FechaHora:dd/MM/yyyy}<br>");
                mensaje.AppendLine($"<b>Total:</b> ${cotizacion.PrecioTotal:F2}<br><br>");
                mensaje.AppendLine("Gracias por confiar en SAGRISA.<br><br>");
                mensaje.AppendLine("Atentamente,<br>Equipo SAGRISA");

                await _emailService.SendEmailAsync(
                    cotizacion.CodClienteNavigation.Correo,
                    asunto,
                    mensaje.ToString()
                );
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error enviando email: {ex.Message}");
            }
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
                PrecioTotal = resumen.Total,
                Estado = "Pendiente"
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

            // Generar PDF
            byte[] pdfBytes = GenerarPdfCotizacion(resumen, cotizacion.CodCotizacion);

            var body = new StringBuilder();
            body.AppendLine($"<div style='font-family: Arial, sans-serif; color: #333;'>");
            body.AppendLine($"<h2 style='color: #2c5282;'>Estimado/a {resumen.NombreCliente},</h2>");
            body.AppendLine($"<p>Nos complace adjuntar su cotización solicitada.</p>");
            body.AppendLine($"<div style='background-color: #f7fafc; padding: 20px; border-left: 4px solid #2c5282; margin: 20px 0;'>");
            body.AppendLine($"<h3 style='margin: 0; color: #2c5282;'>Resumen de Cotización</h3>");
            body.AppendLine($"<p><strong>Número de Cotización:</strong> {cotizacion.CodCotizacion:D6}</p>");
            body.AppendLine($"<p><strong>Fecha:</strong> {resumen.FechaCotizacion}</p>");
            body.AppendLine($"<p><strong>Total de Productos:</strong> {resumen.Productos?.Count ?? 0}</p>");
            body.AppendLine($"<p><strong>Total:</strong> {resumen.Total:C}</p>");
            body.AppendLine($"</div>");
            body.AppendLine($"<p>Para más detalles, consulte el archivo PDF adjunto.</p>");
            body.AppendLine($"<p style='color: #666; font-size: 14px;'>Si tiene alguna pregunta, no dude en contactarnos.</p>");
            body.AppendLine($"<hr style='border: none; border-top: 1px solid #e2e8f0; margin: 30px 0;'>");
            body.AppendLine($"<p style='font-size: 12px; color: #999;'>Gracias por confiar en <strong>SAGRISA</strong></p>");
            body.AppendLine($"</div>");

            // Enviar correo con PDF adjunto
            var attachments = new List<(byte[] content, string fileName, string contentType)>
    {
        (pdfBytes, $"Cotizacion_{cotizacion.CodCotizacion:D6}.pdf", "application/pdf")
    };

            await _emailService.SendEmailWithAttachmentsAsync(
                resumen.CorreoCliente,
                $"Cotización SAGRISA - #{cotizacion.CodCotizacion:D6}",
                body.ToString(),
                attachments
            );

            return View("Confirmacion");
        }

        private byte[] GenerarPdfCotizacion(CotizacionResumenViewModel resumen, int numeroCotizacion)
        {
            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 80, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Fuentes
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(44, 82, 130)); // #2c5282
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);

                var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
                headerTable.SetWidths(new float[] { 3f, 2f });

                var leftCell = new PdfPCell();
                leftCell.Border = Rectangle.NO_BORDER;
                leftCell.AddElement(new Paragraph("SAGRISA", titleFont));
                leftCell.AddElement(new Paragraph("Sistema Agrícola de Riego S.A.", normalFont));
                leftCell.AddElement(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}", normalFont));
                headerTable.AddCell(leftCell);

                var rightCell = new PdfPCell();
                rightCell.Border = Rectangle.NO_BORDER;
                rightCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                rightCell.AddElement(new Paragraph("COTIZACIÓN", headerFont));
                rightCell.AddElement(new Paragraph($"#{numeroCotizacion:D6}", titleFont));
                headerTable.AddCell(rightCell);

                document.Add(headerTable);
                document.Add(new Paragraph(" "));

                var clienteTable = new PdfPTable(2) { WidthPercentage = 100 };
                clienteTable.SetWidths(new float[] { 1f, 1f });

                var clienteHeader = new PdfPCell(new Phrase("INFORMACIÓN DEL CLIENTE", headerFont))
                {
                    Colspan = 2,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    BackgroundColor = new BaseColor(247, 250, 252),
                    Padding = 10,
                    Border = Rectangle.BOX
                };
                clienteTable.AddCell(clienteHeader);

                clienteTable.AddCell(new PdfPCell(new Phrase("Cliente:", boldFont)) { Border = Rectangle.BOX, Padding = 8 });
                clienteTable.AddCell(new PdfPCell(new Phrase(resumen.NombreCliente ?? "", normalFont)) { Border = Rectangle.BOX, Padding = 8 });

                clienteTable.AddCell(new PdfPCell(new Phrase("Empresa:", boldFont)) { Border = Rectangle.BOX, Padding = 8 });
                clienteTable.AddCell(new PdfPCell(new Phrase(resumen.Empresa ?? "", normalFont)) { Border = Rectangle.BOX, Padding = 8 });

                clienteTable.AddCell(new PdfPCell(new Phrase("Correo:", boldFont)) { Border = Rectangle.BOX, Padding = 8 });
                clienteTable.AddCell(new PdfPCell(new Phrase(resumen.CorreoCliente ?? "", normalFont)) { Border = Rectangle.BOX, Padding = 8 });

                document.Add(clienteTable);
                document.Add(new Paragraph(" "));

                if (resumen.Productos != null && resumen.Productos.Any())
                {
                    var productosTable = new PdfPTable(4) { WidthPercentage = 100 };
                    productosTable.SetWidths(new float[] { 3f, 1f, 1.5f, 1.5f });

                    var headers = new string[] { "Producto", "Cantidad", "Precio Unitario", "Subtotal" };
                    foreach (var header in headers)
                    {
                        var headerCell = new PdfPCell(new Phrase(header, boldFont))
                        {
                            BackgroundColor = new BaseColor(44, 82, 130),
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 10
                        };
                        headerCell.Phrase.Font.Color = BaseColor.WHITE;
                        productosTable.AddCell(headerCell);
                    }

                    foreach (var producto in resumen.Productos)
                    {
                        productosTable.AddCell(new PdfPCell(new Phrase(producto.NombreProducto ?? "", normalFont)) { Padding = 8 });
                        productosTable.AddCell(new PdfPCell(new Phrase(producto.Cantidad.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 });
                        productosTable.AddCell(new PdfPCell(new Phrase(producto.Precio.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 8 });
                        productosTable.AddCell(new PdfPCell(new Phrase(producto.Subtotal.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 8 });
                    }

                    document.Add(productosTable);
                }

                document.Add(new Paragraph(" "));

                var totalesTable = new PdfPTable(2) { WidthPercentage = 100 };
                totalesTable.SetWidths(new float[] { 3f, 1f });

                totalesTable.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });

                var resumenTable = new PdfPTable(2) { WidthPercentage = 100 };
                resumenTable.SetWidths(new float[] { 1f, 1f });

                resumenTable.AddCell(new PdfPCell(new Phrase("Subtotal:", boldFont)) { Border = Rectangle.BOX, Padding = 8, HorizontalAlignment = Element.ALIGN_LEFT });
                resumenTable.AddCell(new PdfPCell(new Phrase(resumen.Subtotal.ToString("C"), normalFont)) { Border = Rectangle.BOX, Padding = 8, HorizontalAlignment = Element.ALIGN_RIGHT });

                resumenTable.AddCell(new PdfPCell(new Phrase("IVA (13%):", boldFont)) { Border = Rectangle.BOX, Padding = 8, HorizontalAlignment = Element.ALIGN_LEFT });
                resumenTable.AddCell(new PdfPCell(new Phrase(resumen.IVA.ToString("C"), normalFont)) { Border = Rectangle.BOX, Padding = 8, HorizontalAlignment = Element.ALIGN_RIGHT });

                var totalCell1 = new PdfPCell(new Phrase("TOTAL:", titleFont)) { Border = Rectangle.BOX, Padding = 10, HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new BaseColor(44, 82, 130) };
                totalCell1.Phrase.Font.Color = BaseColor.WHITE;
                resumenTable.AddCell(totalCell1);

                var totalCell2 = new PdfPCell(new Phrase(resumen.Total.ToString("C"), titleFont)) { Border = Rectangle.BOX, Padding = 10, HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = new BaseColor(44, 82, 130) };
                totalCell2.Phrase.Font.Color = BaseColor.WHITE;
                resumenTable.AddCell(totalCell2);

                totalesTable.AddCell(new PdfPCell(resumenTable) { Border = Rectangle.NO_BORDER });
                document.Add(totalesTable);

                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));
                var footerParagraph = new Paragraph("Esta cotización tiene una validez de 30 días a partir de la fecha de emisión.",
                    FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.GRAY));
                footerParagraph.Alignment = Element.ALIGN_CENTER;
                document.Add(footerParagraph);

                document.Close();
                return memoryStream.ToArray();
            }
        }
    }
}
