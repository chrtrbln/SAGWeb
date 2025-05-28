using Microsoft.AspNetCore.Mvc;

namespace SAGWeb.Controllers
{
    public class BaseController : Controller
    {
        protected bool UsuarioAutenticado =>
        HttpContext.Session.GetInt32("CodVendedor") != null;

        protected int CodVendedor => HttpContext.Session.GetInt32("CodVendedor") ?? 0;
        protected string NombreVendedor => HttpContext.Session.GetString("NombreVendedor");
    }
}
