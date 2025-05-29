using System;
using System.Collections.Generic;

namespace SAGWeb.Models;

public partial class Cotizacione
{
    public int CodCotizacion { get; set; }

    public int CodCliente { get; set; }

    public int CodVendedor { get; set; }

    public DateTime? FechaHora { get; set; }

    public decimal? PrecioTotal { get; set; }
    public string? Estado {  get; set; }

    public virtual Cliente CodClienteNavigation { get; set; } = null!;

    public virtual SagusuariosMovil CodVendedorNavigation { get; set; } = null!;

    public virtual ICollection<CotizacionDetalle> CotizacionDetalles { get; set; } = new List<CotizacionDetalle>();
}
