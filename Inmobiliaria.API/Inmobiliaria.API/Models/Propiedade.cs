using System;
using System.Collections.Generic;

namespace Inmobiliaria.API.Models;

public partial class Propiedade
{
    public int PropiedadId { get; set; }

    public string Direccion { get; set; } = null!;

    public decimal PrecioVenta { get; set; }

    public bool? EsBonoVerde { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<Simulacione> Simulaciones { get; set; } = new List<Simulacione>();
}
