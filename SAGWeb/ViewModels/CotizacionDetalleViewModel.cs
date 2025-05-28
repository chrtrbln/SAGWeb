namespace SAGWeb.ViewModels
{
    /*public class CotizacionDetalleViewModel
    {
        public int CodProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }*/

    public class CotizacionDetalleViewModel
    {
        public int CodCotizacion { get; set; }
        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public string NombreVendedor { get; set; }
        public DateTime Fecha { get; set; }
        public List<CotizacionDetalleProductoViewModel> Productos { get; set; }
        public decimal Total { get; set; }
    }

    public class CotizacionDetalleProductoViewModel
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
