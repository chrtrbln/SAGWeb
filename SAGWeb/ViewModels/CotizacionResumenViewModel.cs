namespace SAGWeb.ViewModels
{
    public class CotizacionResumenViewModel
    {
        public string FechaCotizacion { get; set; }
        public string NombreCliente { get; set; }
        public string Empresa { get; set; }
        public string CorreoCliente { get; set; }
        public List<ProductoDetalleViewModel> Productos { get; set; }

        public decimal Subtotal => (decimal)Productos.Sum(p => p.Subtotal);
        public decimal IVA => Subtotal * 0.13m; // Ajusta el IVA si es diferente
        public decimal Total => Subtotal + IVA;
    }

    public class ProductoDetalleViewModel
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public double Subtotal => Cantidad * Precio;
        public float Precio { get; set; }

        public float PrecioBase { get; set; }
    }
}
