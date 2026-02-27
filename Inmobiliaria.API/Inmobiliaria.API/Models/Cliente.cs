using System;
using System.Collections.Generic;

namespace Inmobiliaria.API.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public int? UsuarioId { get; set; }

    public string Dni { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public decimal SueldoMensual { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Simulacione> Simulaciones { get; set; } = new List<Simulacione>();

    public virtual Usuario? Usuario { get; set; }
}
