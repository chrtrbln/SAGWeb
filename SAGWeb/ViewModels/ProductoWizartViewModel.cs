using System.ComponentModel.DataAnnotations;

namespace SAGWeb.ViewModels
{
    public class ProductoWizardViewModel
    {
        [Required]
        public string NombreProducto { get; set; }

        public string Bodega { get; set; }

        public int Inventario { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Cantidad debe ser mayor que 0")]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debe ingresar un precio válido")]
        public float Precio { get; set; }

        public float PrecioBase { get; set; }
    }
}
