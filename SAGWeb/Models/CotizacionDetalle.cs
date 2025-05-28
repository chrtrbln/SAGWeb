using System;
using System.Collections.Generic;

namespace SAGWeb.Models;

public partial class CotizacionDetalle
{
    public int IdDetalle { get; set; }

    public int CodCotizacion { get; set; }

    public int CodProducto { get; set; }

    public string? NombreProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual Cotizacione CodCotizacionNavigation { get; set; } = null!;

    public virtual SagpreciosEnLinea CodProductoNavigation { get; set; } = null!;
}

