using System;
using System.Collections.Generic;

namespace SAGWeb.Models;

public partial class SagusuariosMovil
{
    public string? Pin { get; set; }

    public string? Nombre { get; set; }

    public string? Pais { get; set; }

    public int CodVendedor { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();
}
