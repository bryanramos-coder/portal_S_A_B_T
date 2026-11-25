using System;
using System.Collections.Generic;

namespace PortalGraduados.Api.Models;

public partial class DatosRegistro
{
    public int IdUsuario { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? FotoUrl { get; set; }

    public string? Cargo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
