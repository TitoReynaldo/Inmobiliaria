using System;
using System.Collections.Generic;

namespace Inmobiliaria.API.Models;

public partial class Maestrobono
{
    public int BonoId { get; set; }

    public decimal PrecioViviendaDesde { get; set; }

    public decimal PrecioViviendaHasta { get; set; }

    public decimal MontoBono { get; set; }

    public bool EsSostenible { get; set; }
}
