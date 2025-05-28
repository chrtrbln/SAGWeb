using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAGWeb.Models;

public partial class Cliente
{
    [Key]
    public int CodCliente { get; set; }

    public string? NomCliente { get; set; }

    public string? Clase { get; set; }

    public int? Vendedor { get; set; }

    public string? Ciudad { get; set; }

    public string? Tpago { get; set; }

    public string? Lprecios { get; set; }

    public decimal? MontoCredito { get; set; }

    public decimal? TotalDeuda { get; set; }

    public decimal? SaldoCredito { get; set; }

    public string? Correo { get; set; }

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();

    public virtual SagusuariosMovil? VendedorNavigation { get; set; }
}
