using System;
using System.Collections.Generic;

namespace SAGWeb.Models;

public partial class SagpreciosEnLinea
{
    public int CodProducto { get; set; }

    public string? NomProducto { get; set; }

    public string? Bodega { get; set; }

    public int? Existencia { get; set; }

    public decimal? Pbase { get; set; }

    public decimal? Costo { get; set; }

    public decimal? Peso { get; set; }

    public string? ListaPrecio { get; set; }

    public decimal? PorcentajeDesc { get; set; }

    public decimal? PrecioVenta { get; set; }

    public string? Pais { get; set; }

    public string? Clase { get; set; }

    public decimal? PrecioSinIva { get; set; }

    public int? CantDecimales { get; set; }

    public virtual ICollection<CotizacionDetalle> CotizacionDetalles { get; set; } = new List<CotizacionDetalle>();
}
