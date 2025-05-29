namespace SAGWeb.ViewModels
{
    public class CotizacionListadoViewModel
    {
        public int CodCotizacion { get; set; }
        public string NombreCliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal PrecioTotal { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }
}
